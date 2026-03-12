using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions;

internal static class PromotionValidation
{
    public static PromotionInput NormalizeAndValidate(PromotionDto promotion)
    {
        if (promotion.DiscountPercentage <= 0 || promotion.DiscountPercentage > 100)
        {
            throw new ArgumentException("DiscountPercentage must be between 0 and 100.");
        }

        if (promotion.EndsAtUtc is not null && promotion.StartsAtUtc is not null && promotion.EndsAtUtc < promotion.StartsAtUtc)
        {
            throw new ArgumentException("EndsAtUtc cannot be earlier than StartsAtUtc.");
        }

        var normalizedProductIds = (promotion.ProductIds ?? [])
            .Where(productId => productId != Guid.Empty)
            .Distinct()
            .ToArray();

        switch (promotion.Type)
        {
            case PromotionTypeDto.ProductDiscount:
                if (normalizedProductIds.Length == 0)
                {
                    throw new ArgumentException("ProductDiscount promotion requires at least one ProductId.");
                }

                if (promotion.RequiredPoints is not null)
                {
                    throw new ArgumentException("ProductDiscount promotion cannot set RequiredPoints.");
                }

                return new PromotionInput(
                    PromotionType.ProductDiscount,
                    promotion.DiscountPercentage,
                    normalizedProductIds,
                    promotion.StartsAtUtc,
                    promotion.EndsAtUtc,
                    null);

            case PromotionTypeDto.LoyaltyPoints:
                if (promotion.RequiredPoints is null || promotion.RequiredPoints <= 0)
                {
                    throw new ArgumentException("LoyaltyPoints promotion requires RequiredPoints greater than 0.");
                }

                if (normalizedProductIds.Length > 0)
                {
                    throw new ArgumentException("LoyaltyPoints promotion cannot target ProductIds.");
                }

                return new PromotionInput(
                    PromotionType.LoyaltyPoints,
                    promotion.DiscountPercentage,
                    [],
                    promotion.StartsAtUtc,
                    promotion.EndsAtUtc,
                    promotion.RequiredPoints.Value);

            default:
                throw new ArgumentOutOfRangeException(nameof(promotion.Type), promotion.Type, "Unknown promotion type.");
        }
    }
}

internal sealed record PromotionInput(
    PromotionType Type,
    decimal DiscountPercentage,
    Guid[] ProductIds,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    decimal? RequiredPoints);
