namespace PromotionService.Contracts.Dtos;

public sealed record UserPromotionProfileDto(
    Guid UserId,
    decimal LoyaltyPoints,
    int OrdersCount,
    decimal TotalSpent,
    DateTime? LastOrderAtUtc);
