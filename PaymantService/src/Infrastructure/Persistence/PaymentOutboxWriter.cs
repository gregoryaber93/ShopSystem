using PaymantService.Application.Abstractions.Persistence;
using PaymantService.Domain.Entities;

namespace PaymantService.Infrastructure.Persistence;

public sealed class PaymentOutboxWriter(PaymentDbContext dbContext) : IPaymentOutboxWriter
{
    public Task EnqueueAsync(
        Guid eventId,
        string eventType,
        string payload,
        string partitionKey,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken)
    {
        return dbContext.PaymentOutboxMessages.AddAsync(new PaymentOutboxMessageEntity
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            EventType = eventType,
            Payload = payload,
            PartitionKey = partitionKey,
            OccurredOnUtc = occurredOnUtc,
            CreatedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        }, cancellationToken).AsTask();
    }
}
