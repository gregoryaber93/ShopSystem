namespace OrderService.Application.Abstractions.Persistence;

public interface IOrderOutboxWriter
{
    Task EnqueueAsync(
        Guid eventId,
        string eventType,
        string payload,
        string partitionKey,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken);
}
