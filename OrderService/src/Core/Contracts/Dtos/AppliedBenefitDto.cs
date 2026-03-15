namespace OrderService.Contracts.Dtos;

public sealed record AppliedBenefitDto(
    Guid PromotionId,
    string Type,
    decimal DiscountPercentage,
    string Reason);
