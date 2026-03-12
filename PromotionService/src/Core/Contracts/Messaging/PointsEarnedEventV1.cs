namespace PromotionService.Contracts.Messaging;

public sealed record PointsEarnedEventV1(
    Guid UserId,
    decimal Points,
    DateTime OccurredOnUtc);
