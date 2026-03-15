namespace OrderService.Application.Abstractions.Integrations;

public sealed record AppliedPromotionSnapshot(
    Guid PromotionId,
    string Type,
    decimal DiscountPercentage,
    string Reason);

public sealed record PromotionEvaluationSnapshot(
    Guid UserId,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal FinalPrice,
    IReadOnlyCollection<AppliedPromotionSnapshot> AppliedPromotions);
