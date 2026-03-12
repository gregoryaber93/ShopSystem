using OrderService.Application.Abstractions.Persistence;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public sealed class OrderOutboxWriter(OrderDbContext dbContext) : IOrderOutboxWriter
{
    public Task EnqueueAsync(
        Guid eventId,
        string eventType,
        string payload,
        string partitionKey,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken)
    {
        return dbContext.OrderOutboxMessages.AddAsync(new OrderOutboxMessageEntity
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
