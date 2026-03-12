using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Queries.GetPromotions;

public sealed class GetPromotionsQueryHandler(IPromotionRepository promotionRepository) : IQueryHandler<GetPromotionsQuery, IReadOnlyCollection<PromotionDto>>
{
    public async Task<IReadOnlyCollection<PromotionDto>> Handle(GetPromotionsQuery query, CancellationToken cancellationToken)
    {
        var promotions = await promotionRepository.GetAllAsync(cancellationToken);

        return promotions
            .Select(promotion => new PromotionDto(
                promotion.Id,
                MapType(promotion.Type),
                promotion.DiscountPercentage,
                promotion.ProductIds ?? [],
                promotion.StartsAtUtc,
                promotion.EndsAtUtc,
                promotion.RequiredPoints))
            .ToArray();
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
