namespace PromotionService.Contracts.Dtos;

public sealed record AppliedPromotionDto(
    Guid PromotionId,
    PromotionTypeDto Type,
    decimal DiscountPercentage,
    string Reason);

public sealed record PromotionEvaluationResultDto(
    Guid UserId,
    decimal Subtotal,
    decimal TotalDiscountPercentage,
    decimal DiscountAmount,
    decimal FinalPrice,
    IReadOnlyCollection<AppliedPromotionDto> AppliedPromotions);
