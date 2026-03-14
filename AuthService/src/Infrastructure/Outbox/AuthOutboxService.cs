using System.Text.Json;
using AuthenticationService.Application.Abstractions.Outbox;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Infrastructure.Outbox;

internal sealed class AuthOutboxService(AuthDbContext dbContext) : IAuthOutboxService
{
    private const string UserCreatedEventType = "UserCreated";

    public async Task<Guid> EnqueueUserCreatedAsync(Guid userId, string email, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        var payload = new OutboxUserCreatedPayload(userId, email, roles);
        var message = new AuthOutboxMessageEntity
        {
            Id = Guid.NewGuid(),
            EventType = UserCreatedEventType,
            PayloadJson = JsonSerializer.Serialize(payload),
            Status = AuthOutboxStatus.Pending,
            RetryCount = 0,
            CreatedAtUtc = DateTime.UtcNow,
            NextAttemptAtUtc = DateTime.UtcNow
        };

        await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
        return message.Id;
    }

    public async Task MarkCompletedAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages.FirstOrDefaultAsync(item => item.Id == messageId, cancellationToken)
            ?? throw new InvalidOperationException($"Outbox message '{messageId}' was not found.");

        message.Status = AuthOutboxStatus.Completed;
        message.ProcessedAtUtc = DateTime.UtcNow;
        message.LastError = null;
        message.NextAttemptAtUtc = DateTime.UtcNow;
    }

    public async Task MarkRetryableFailureAsync(Guid messageId, string error, DateTime nextAttemptAtUtc, CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages.FirstOrDefaultAsync(item => item.Id == messageId, cancellationToken)
            ?? throw new InvalidOperationException($"Outbox message '{messageId}' was not found.");

        message.Status = AuthOutboxStatus.FailedRetryable;
        message.RetryCount += 1;
        message.LastError = error;
        message.NextAttemptAtUtc = nextAttemptAtUtc;
    }

    public async Task MarkPermanentFailureAsync(Guid messageId, string error, CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages.FirstOrDefaultAsync(item => item.Id == messageId, cancellationToken)
            ?? throw new InvalidOperationException($"Outbox message '{messageId}' was not found.");

        message.Status = AuthOutboxStatus.FailedPermanent;
        message.ProcessedAtUtc = DateTime.UtcNow;
        message.LastError = error;
        message.NextAttemptAtUtc = DateTime.UtcNow;
    }
}
