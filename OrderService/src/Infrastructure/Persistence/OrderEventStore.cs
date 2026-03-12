using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public sealed class OrderEventStore(OrderDbContext dbContext) : IOrderEventStore
{
    public async Task<int> GetLatestVersionAsync(Guid aggregateId, CancellationToken cancellationToken)
    {
        var latest = await dbContext.OrderEventStreams
            .Where(stream => stream.AggregateId == aggregateId)
            .Select(stream => (int?)stream.Version)
            .MaxAsync(cancellationToken);

        return latest ?? 0;
    }

    public async Task AppendAsync(
        Guid aggregateId,
        string eventType,
        int eventVersion,
        string payload,
        DateTime occurredOnUtc,
        int expectedVersion,
        CancellationToken cancellationToken)
    {
        var latestVersion = await GetLatestVersionAsync(aggregateId, cancellationToken);
        if (latestVersion != expectedVersion)
        {
            throw new InvalidOperationException($"Order aggregate version conflict. Expected {expectedVersion}, actual {latestVersion}.");
        }

        var nextVersion = latestVersion + 1;
        await dbContext.OrderEventStreams.AddAsync(new OrderEventStreamEntity
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregateId,
            AggregateType = "Order",
            Version = nextVersion,
            EventType = eventType,
            EventVersion = eventVersion,
            Payload = payload,
            OccurredOnUtc = occurredOnUtc,
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrderEventStreamEntity>> LoadEventsAsync(
        Guid aggregateId,
        int fromExclusiveVersion,
        CancellationToken cancellationToken)
    {
        return await dbContext.OrderEventStreams
            .AsNoTracking()
            .Where(stream => stream.AggregateId == aggregateId && stream.Version > fromExclusiveVersion)
            .OrderBy(stream => stream.Version)
            .ToListAsync(cancellationToken);
    }

    public Task<OrderSnapshotEntity?> GetLatestSnapshotAsync(Guid aggregateId, CancellationToken cancellationToken)
    {
        return dbContext.OrderSnapshots
            .AsNoTracking()
            .Where(snapshot => snapshot.AggregateId == aggregateId)
            .OrderByDescending(snapshot => snapshot.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveSnapshotAsync(OrderSnapshotEntity snapshot, CancellationToken cancellationToken)
    {
        var existing = await dbContext.OrderSnapshots
            .FirstOrDefaultAsync(item => item.AggregateId == snapshot.AggregateId, cancellationToken);

        if (existing is null)
        {
            await dbContext.OrderSnapshots.AddAsync(snapshot, cancellationToken);
            return;
        }

        if (existing.Version >= snapshot.Version)
        {
            return;
        }

        existing.Version = snapshot.Version;
        existing.Payload = snapshot.Payload;
        existing.CreatedAtUtc = snapshot.CreatedAtUtc;
    }
}
