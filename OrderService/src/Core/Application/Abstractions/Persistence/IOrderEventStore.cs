using OrderService.Domain.Entities;

namespace OrderService.Application.Abstractions.Persistence;

public interface IOrderEventStore
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
    Task<IReadOnlyCollection<OrderEventStreamEntity>> LoadEventsAsync(
        Guid aggregateId,
        int fromExclusiveVersion,
        CancellationToken cancellationToken);
    Task<OrderSnapshotEntity?> GetLatestSnapshotAsync(Guid aggregateId, CancellationToken cancellationToken);
    Task SaveSnapshotAsync(OrderSnapshotEntity snapshot, CancellationToken cancellationToken);
}
