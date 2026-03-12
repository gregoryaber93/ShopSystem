namespace PaymantService.Domain.Entities;

public sealed class PaymentProcessedEventEntity
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
