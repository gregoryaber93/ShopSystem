using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Caching;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions;
public sealed class EvaluatePromotionsQueryHandler(
    IPromotionCacheService promotionCacheService,
    IPromotionRepository promotionRepository,
    IUserPromotionProfileRepository userPromotionProfileRepository) : IQueryHandler<EvaluatePromotionsQuery, PromotionEvaluationResultDto>
{
    public async Task<PromotionEvaluationResultDto> Handle(EvaluatePromotionsQuery query, CancellationToken cancellationToken)
    {
        Validate(query.Request);

        var normalizedProductIds = (query.Request.ProductIds ?? [])
            .Where(productId => productId != Guid.Empty)
            .OrderBy(productId => productId)
            .ToArray();

        var cachedEligibility = await promotionCacheService.GetEligibilityAsync(
            query.Request.UserId,
            normalizedProductIds,
            query.Request.Subtotal,
            cancellationToken);
        if (cachedEligibility is not null)
        {
            return cachedEligibility;
        }

        var evaluatedAtUtc = query.Request.EvaluatedAtUtc ?? DateTime.UtcNow;
        var requestedProductIds = normalizedProductIds
            .ToHashSet();

        var promotions = await promotionRepository.GetAllAsync(cancellationToken);
        var userProfile = await userPromotionProfileRepository.GetByUserIdAsync(query.Request.UserId, cancellationToken);
        var loyaltyPoints = userProfile?.LoyaltyPoints ?? 0m;

        var applied = new List<AppliedPromotionDto>();
        foreach (var promotion in promotions.OrderBy(p => p.Id))
        {
            if (!IsActive(promotion, evaluatedAtUtc))
            {
                continue;
            }

            if (promotion.Type == PromotionType.ProductDiscount && promotion.ProductIds.Any(requestedProductIds.Contains))
            {
                applied.Add(new AppliedPromotionDto(
                    promotion.Id,
                    PromotionTypeDto.ProductDiscount,
                    promotion.DiscountPercentage,
                    "Product in cart matched promotion."));
            }

            if (promotion.Type == PromotionType.LoyaltyPoints
                && promotion.RequiredPoints is not null
                && loyaltyPoints >= promotion.RequiredPoints.Value)
            {
                applied.Add(new AppliedPromotionDto(
                    promotion.Id,
                    PromotionTypeDto.LoyaltyPoints,
                    promotion.DiscountPercentage,
                    "User loyalty points threshold met."));
            }
        }

        var totalDiscountPercentage = decimal.Min(100m, applied.Sum(item => item.DiscountPercentage));
        var discountAmount = decimal.Round(
            query.Request.Subtotal * (totalDiscountPercentage / 100m),
            2,
            MidpointRounding.AwayFromZero);
        var finalPrice = decimal.Round(
            query.Request.Subtotal - discountAmount,
            2,
            MidpointRounding.AwayFromZero);

        if (finalPrice < 0)
        {
            finalPrice = 0;
        }

        var result = new PromotionEvaluationResultDto(
            query.Request.UserId,
            query.Request.Subtotal,
            totalDiscountPercentage,
            discountAmount,
            finalPrice,
            applied);

        await promotionCacheService.SetEligibilityAsync(
            query.Request.UserId,
            normalizedProductIds,
            query.Request.Subtotal,
            result,
            TimeSpan.FromMinutes(2),
            cancellationToken);

        return result;
    }

    private static bool IsActive(PromotionEntity promotion, DateTime evaluatedAtUtc)
    {
        var started = promotion.StartsAtUtc is null || promotion.StartsAtUtc.Value <= evaluatedAtUtc;
        var notEnded = promotion.EndsAtUtc is null || promotion.EndsAtUtc.Value >= evaluatedAtUtc;
        return started && notEnded;
    }

    private static void Validate(PromotionEvaluationRequestDto request)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.");
        }

        if (request.Subtotal < 0)
        {
            throw new ArgumentException("Subtotal cannot be negative.");
        }
    }
}
