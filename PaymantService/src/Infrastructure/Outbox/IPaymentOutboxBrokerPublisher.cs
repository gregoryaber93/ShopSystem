using PaymantService.Domain.Entities;

namespace PaymantService.Infrastructure.Outbox;

public interface IPaymentOutboxBrokerPublisher
{
    Task PublishAsync(PaymentOutboxMessageEntity message, CancellationToken cancellationToken);
    Task PublishDeadLetterAsync(PaymentOutboxMessageEntity message, CancellationToken cancellationToken);
}
