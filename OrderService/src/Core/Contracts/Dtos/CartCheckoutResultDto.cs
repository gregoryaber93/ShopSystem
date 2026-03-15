namespace OrderService.Contracts.Dtos;

public sealed record CartCheckoutResultDto(
    Guid UserId,
    Guid ShopId,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal FinalPrice,
    int EarnedLoyaltyPoints,
    bool PromotionAccepted,
    IReadOnlyCollection<AppliedBenefitDto> AppliedBenefits,
    IReadOnlyCollection<CartOrderLineDto> Orders);
