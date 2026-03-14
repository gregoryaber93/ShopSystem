namespace UserService.Domain.Entities;

public sealed class UserProcessedEventEntity
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
