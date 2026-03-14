namespace AuthenticationService.Application.Abstractions.Outbox;

public interface IAuthOutboxService
{
    Task<Guid> EnqueueUserCreatedAsync(Guid userId, string email, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);
    Task MarkCompletedAsync(Guid messageId, CancellationToken cancellationToken);
    Task MarkRetryableFailureAsync(Guid messageId, string error, DateTime nextAttemptAtUtc, CancellationToken cancellationToken);
    Task MarkPermanentFailureAsync(Guid messageId, string error, CancellationToken cancellationToken);
}
