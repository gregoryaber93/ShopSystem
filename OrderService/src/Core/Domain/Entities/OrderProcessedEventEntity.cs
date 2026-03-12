namespace OrderService.Domain.Entities;

public sealed class OrderProcessedEventEntity
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
