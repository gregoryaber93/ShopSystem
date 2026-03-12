using Microsoft.EntityFrameworkCore;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Domain.Entities;

namespace PromotionService.Infrastructure.Persistence;

public sealed class LoyaltyEventStore(PromotionDbContext dbContext) : ILoyaltyEventStore
{
    public async Task<int> GetLatestVersionAsync(Guid aggregateId, CancellationToken cancellationToken)
    {
        var latest = await dbContext.LoyaltyEventStreams
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
            throw new InvalidOperationException($"Loyalty aggregate version conflict. Expected {expectedVersion}, actual {latestVersion}.");
        }

        var nextVersion = latestVersion + 1;
        await dbContext.LoyaltyEventStreams.AddAsync(new LoyaltyEventStreamEntity
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregateId,
            AggregateType = "LoyaltyLedger",
            Version = nextVersion,
            EventType = eventType,
            EventVersion = eventVersion,
            Payload = payload,
            OccurredOnUtc = occurredOnUtc,
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoyaltyEventStreamEntity>> LoadEventsAsync(
        Guid aggregateId,
        int fromExclusiveVersion,
        CancellationToken cancellationToken)
    {
        return await dbContext.LoyaltyEventStreams
            .AsNoTracking()
            .Where(stream => stream.AggregateId == aggregateId && stream.Version > fromExclusiveVersion)
            .OrderBy(stream => stream.Version)
            .ToListAsync(cancellationToken);
    }

    public Task<LoyaltySnapshotEntity?> GetLatestSnapshotAsync(Guid aggregateId, CancellationToken cancellationToken)
    {
        return dbContext.LoyaltySnapshots
            .AsNoTracking()
            .Where(snapshot => snapshot.AggregateId == aggregateId)
            .OrderByDescending(snapshot => snapshot.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveSnapshotAsync(LoyaltySnapshotEntity snapshot, CancellationToken cancellationToken)
    {
        var existing = await dbContext.LoyaltySnapshots
            .FirstOrDefaultAsync(item => item.AggregateId == snapshot.AggregateId, cancellationToken);

        if (existing is null)
        {
            await dbContext.LoyaltySnapshots.AddAsync(snapshot, cancellationToken);
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
