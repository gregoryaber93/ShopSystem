using PromotionService.Contracts.Messaging;

namespace PromotionService.Application.Features.Promotions.EventSourcing;

public sealed class LoyaltyAggregateState
{
    public Guid UserId { get; private set; }
    public decimal LoyaltyPoints { get; private set; }
    public int OrdersCount { get; private set; }
    public decimal TotalSpent { get; private set; }
    public DateTime? LastOrderAtUtc { get; private set; }
    public int Version { get; private set; }

    public void Apply(PointsEarnedEventV1 @event, int version)
    {
        UserId = @event.UserId;
        LoyaltyPoints += @event.Points;
        Version = version;
    }

    public void Apply(PointsSpentEventV1 @event, int version)
    {
        UserId = @event.UserId;
        LoyaltyPoints = decimal.Max(0, LoyaltyPoints - @event.Points);
        Version = version;
    }

    public void Apply(LoyaltyProfileUpdatedEventV1 @event, int version)
    {
        UserId = @event.UserId;
        LoyaltyPoints = @event.LoyaltyPoints;
        OrdersCount = @event.OrdersCount;
        TotalSpent = @event.TotalSpent;
        LastOrderAtUtc = @event.LastOrderAtUtc;
        Version = version;
    }

    public Snapshot ToSnapshot()
    {
        return new Snapshot(UserId, LoyaltyPoints, OrdersCount, TotalSpent, LastOrderAtUtc, Version);
    }

    public void ApplySnapshot(Snapshot snapshot)
    {
        UserId = snapshot.UserId;
        LoyaltyPoints = snapshot.LoyaltyPoints;
        OrdersCount = snapshot.OrdersCount;
        TotalSpent = snapshot.TotalSpent;
        LastOrderAtUtc = snapshot.LastOrderAtUtc;
        Version = snapshot.Version;
    }

    public sealed record Snapshot(
        Guid UserId,
        decimal LoyaltyPoints,
        int OrdersCount,
        decimal TotalSpent,
        DateTime? LastOrderAtUtc,
        int Version);
}
