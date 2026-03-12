namespace OrderService.Domain.Entities;

public sealed class OrderOutboxMessageEntity
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string PartitionKey { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? NextRetryAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public DateTime? DeadLetteredAtUtc { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
}
