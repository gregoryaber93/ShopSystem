namespace PromotionService.Contracts.Messaging;

public sealed record PointsSpentEventV1(
    Guid UserId,
    decimal Points,
    DateTime OccurredOnUtc);
