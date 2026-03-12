using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Caching;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Queries.GetPromotions;
public sealed class GetPromotionsQueryHandler(
    IPromotionRepository promotionRepository,
    IPromotionCacheService promotionCacheService) : IQueryHandler<GetPromotionsQuery, IReadOnlyCollection<PromotionDto>>
{
    public async Task<IReadOnlyCollection<PromotionDto>> Handle(GetPromotionsQuery query, CancellationToken cancellationToken)
    {
        var cached = await promotionCacheService.GetActivePromotionsAsync(cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var promotions = await promotionRepository.GetAllAsync(cancellationToken);

        var result = promotions
            .Select(promotion => new PromotionDto(
                promotion.Id,
                MapType(promotion.Type),
                promotion.DiscountPercentage,
                promotion.ProductIds ?? [],
                promotion.StartsAtUtc,
                promotion.EndsAtUtc,
                promotion.RequiredPoints))
            .ToArray();

            await promotionCacheService.SetActivePromotionsAsync(result, TimeSpan.FromMinutes(5), cancellationToken);
            return result;
    }

    private static PromotionTypeDto MapType(PromotionType type)
    {
        return type switch
        {
            PromotionType.ProductDiscount => PromotionTypeDto.ProductDiscount,
            PromotionType.LoyaltyPoints => PromotionTypeDto.LoyaltyPoints,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown promotion type.")
        };
    }
}
