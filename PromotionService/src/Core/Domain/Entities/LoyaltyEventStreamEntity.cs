namespace PromotionService.Domain.Entities;

public sealed class LoyaltyEventStreamEntity
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = "LoyaltyLedger";
    public int Version { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int EventVersion { get; set; } = 1;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
