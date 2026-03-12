namespace PromotionService.Domain.Entities;

public sealed class LoyaltySnapshotEntity
{
    public Guid AggregateId { get; set; }
    public int Version { get; set; }
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
