using AuthenticationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthenticationService.Infrastructure.Outbox;

internal sealed class AuthOutboxWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<AuthOutboxOptions> options,
    ILogger<AuthOutboxWorker> logger) : BackgroundService
{
    private readonly AuthOutboxOptions outboxOptions = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled exception in AuthOutboxWorker.");
            }

            await Task.Delay(TimeSpan.FromSeconds(outboxOptions.PollIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var brokerPublisher = scope.ServiceProvider.GetRequiredService<IAuthOutboxBrokerPublisher>();

        var now = DateTime.UtcNow;
        var batch = await dbContext.OutboxMessages
            .Where(message => message.EventType == "UserCreated")
            .Where(message => message.Status == AuthOutboxStatus.Pending || message.Status == AuthOutboxStatus.FailedRetryable)
            .Where(message => message.NextAttemptAtUtc <= now)
            .OrderBy(message => message.CreatedAtUtc)
            .Take(outboxOptions.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in batch)
        {
            try
            {
                await brokerPublisher.PublishAsync(message, cancellationToken);

                message.Status = AuthOutboxStatus.Completed;
                message.ProcessedAtUtc = DateTime.UtcNow;
                message.LastError = null;
                message.NextAttemptAtUtc = DateTime.UtcNow;
            }
            catch (Exception exception)
            {
                message.Status = AuthOutboxStatus.FailedRetryable;
                message.RetryCount += 1;
                message.LastError = exception.Message;

                if (message.RetryCount >= outboxOptions.MaxRetries)
                {
                    await brokerPublisher.PublishDeadLetterAsync(message, cancellationToken);
                    message.Status = AuthOutboxStatus.FailedPermanent;
                    message.ProcessedAtUtc = DateTime.UtcNow;
                    message.NextAttemptAtUtc = DateTime.UtcNow;
                }
                else
                {
                    var backoffSeconds = Math.Min(300, (int)Math.Pow(2, Math.Min(message.RetryCount, 8)));
                    message.NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(backoffSeconds);
                }
            }
        }

        if (batch.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
