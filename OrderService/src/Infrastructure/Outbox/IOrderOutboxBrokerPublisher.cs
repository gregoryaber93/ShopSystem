using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Outbox;

public interface IOrderOutboxBrokerPublisher
{
    Task PublishAsync(OrderOutboxMessageEntity message, CancellationToken cancellationToken);
    Task PublishDeadLetterAsync(OrderOutboxMessageEntity message, CancellationToken cancellationToken);
}
