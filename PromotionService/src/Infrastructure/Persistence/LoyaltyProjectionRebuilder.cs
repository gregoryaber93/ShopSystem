using System.Text.Json;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Application.Features.Promotions.EventSourcing;
using PromotionService.Contracts.Dtos;
using PromotionService.Contracts.Messaging;
using PromotionService.Domain.Entities;

namespace PromotionService.Infrastructure.Persistence;

public sealed class LoyaltyProjectionRebuilder(
    ILoyaltyEventStore loyaltyEventStore,
    IUserPromotionProfileRepository userPromotionProfileRepository) : ILoyaltyProjectionRebuilder
{
    public async Task<UserPromotionProfileDto?> RebuildAsync(Guid userId, CancellationToken cancellationToken)
    {
        var aggregate = new LoyaltyAggregateState();
        var snapshot = await loyaltyEventStore.GetLatestSnapshotAsync(userId, cancellationToken);
        var fromVersion = 0;

        if (snapshot is not null)
        {
            var snapshotModel = JsonSerializer.Deserialize<LoyaltyAggregateState.Snapshot>(snapshot.Payload);
            if (snapshotModel is not null)
            {
                aggregate.ApplySnapshot(snapshotModel);
                fromVersion = snapshot.Version;
            }
        }

        var stream = await loyaltyEventStore.LoadEventsAsync(userId, fromVersion, cancellationToken);
        foreach (var @event in stream)
        {
            if (@event.EventType == "PointsEarned" && @event.EventVersion == 1)
            {
                var payload = JsonSerializer.Deserialize<PointsEarnedEventV1>(@event.Payload);
                if (payload is not null)
                {
                    aggregate.Apply(payload, @event.Version);
                }
            }
            else if (@event.EventType == "PointsSpent" && @event.EventVersion == 1)
            {
                var payload = JsonSerializer.Deserialize<PointsSpentEventV1>(@event.Payload);
                if (payload is not null)
                {
                    aggregate.Apply(payload, @event.Version);
                }
            }
            else if (@event.EventType == "LoyaltyProfileUpdated" && @event.EventVersion == 1)
            {
                var payload = JsonSerializer.Deserialize<LoyaltyProfileUpdatedEventV1>(@event.Payload);
                if (payload is not null)
                {
                    aggregate.Apply(payload, @event.Version);
                }
            }
        }

        if (aggregate.UserId == Guid.Empty)
        {
            return null;
        }

        var projection = new UserPromotionProfileEntity
        {
            UserId = aggregate.UserId,
            LoyaltyPoints = aggregate.LoyaltyPoints,
            OrdersCount = aggregate.OrdersCount,
            TotalSpent = aggregate.TotalSpent,
            LastOrderAtUtc = aggregate.LastOrderAtUtc
        };

        await userPromotionProfileRepository.UpsertAsync(projection, cancellationToken);
        await userPromotionProfileRepository.SaveChangesAsync(cancellationToken);

        return new UserPromotionProfileDto(
            aggregate.UserId,
            aggregate.LoyaltyPoints,
            aggregate.OrdersCount,
            aggregate.TotalSpent,
            aggregate.LastOrderAtUtc);
    }
}
