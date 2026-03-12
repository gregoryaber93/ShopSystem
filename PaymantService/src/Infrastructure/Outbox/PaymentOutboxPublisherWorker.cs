using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymantService.Infrastructure.Persistence;

namespace PaymantService.Infrastructure.Outbox;

public sealed class PaymentOutboxPublisherWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<PaymentOutboxOptions> options,
    ILogger<PaymentOutboxPublisherWorker> logger) : BackgroundService
{
    private readonly PaymentOutboxOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Payment outbox worker iteration failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        var brokerPublisher = scope.ServiceProvider.GetRequiredService<IPaymentOutboxBrokerPublisher>();

        var now = DateTime.UtcNow;
        var pending = await dbContext.PaymentOutboxMessages
            .Where(message => message.ProcessedAtUtc == null
                              && message.DeadLetteredAtUtc == null
                              && (message.NextRetryAtUtc == null || message.NextRetryAtUtc <= now))
            .OrderBy(message => message.CreatedAtUtc)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            var isProcessed = await dbContext.PaymentProcessedEvents
                .AnyAsync(processed => processed.EventId == message.EventId, cancellationToken);

            if (isProcessed)
            {
                message.ProcessedAtUtc = now;
                continue;
            }

            try
            {
                await brokerPublisher.PublishAsync(message, cancellationToken);

                dbContext.PaymentProcessedEvents.Add(new Domain.Entities.PaymentProcessedEventEntity
                {
                    EventId = message.EventId,
                    ProcessedAtUtc = now
                });

                message.ProcessedAtUtc = now;
                message.LastError = null;
            }
            catch (Exception exception)
            {
                message.RetryCount += 1;
                message.LastError = exception.Message[..Math.Min(1000, exception.Message.Length)];

                if (message.RetryCount >= _options.MaxRetries)
                {
                    await brokerPublisher.PublishDeadLetterAsync(message, cancellationToken);
                    message.DeadLetteredAtUtc = now;
                    message.ProcessedAtUtc = now;
                }
                else
                {
                    var retryDelaySeconds = Math.Min(60, (int)Math.Pow(2, message.RetryCount));
                    message.NextRetryAtUtc = now.AddSeconds(retryDelaySeconds);
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
