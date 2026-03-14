using AuthService.Domain.Entities;

namespace AuthService.Infrastructure.Outbox;

public interface IAuthOutboxBrokerPublisher
{
    Task PublishAsync(AuthOutboxMessageEntity message, CancellationToken cancellationToken);
    Task PublishDeadLetterAsync(AuthOutboxMessageEntity message, CancellationToken cancellationToken);
}
