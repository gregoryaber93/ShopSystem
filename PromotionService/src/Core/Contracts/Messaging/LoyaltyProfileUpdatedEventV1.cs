namespace PromotionService.Contracts.Messaging;

public sealed record LoyaltyProfileUpdatedEventV1(
    Guid UserId,
    decimal LoyaltyPoints,
    int OrdersCount,
    decimal TotalSpent,
    DateTime? LastOrderAtUtc,
    DateTime OccurredOnUtc);
