using PromotionService.Domain.Entities;

namespace PromotionService.Application.Abstractions.Persistence;

public interface ILoyaltyEventStore
{
    Task<int> GetLatestVersionAsync(Guid aggregateId, CancellationToken cancellationToken);
    Task AppendAsync(
        Guid aggregateId,
        string eventType,
        int eventVersion,
        string payload,
        DateTime occurredOnUtc,
        int expectedVersion,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LoyaltyEventStreamEntity>> LoadEventsAsync(
        Guid aggregateId,
        int fromExclusiveVersion,
        CancellationToken cancellationToken);
    Task<LoyaltySnapshotEntity?> GetLatestSnapshotAsync(Guid aggregateId, CancellationToken cancellationToken);
    Task SaveSnapshotAsync(LoyaltySnapshotEntity snapshot, CancellationToken cancellationToken);
}
