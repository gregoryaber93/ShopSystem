namespace PaymantService.Application.Abstractions.Persistence;

public interface IPaymentOutboxWriter
{
    Task EnqueueAsync(
        Guid eventId,
        string eventType,
        string payload,
        string partitionKey,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken);
}
