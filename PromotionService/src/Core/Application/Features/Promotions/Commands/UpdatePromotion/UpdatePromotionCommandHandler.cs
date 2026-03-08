using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Commands.UpdatePromotion;

public sealed class UpdatePromotionCommandHandler(IPromotionRepository promotionRepository) : ICommandHandler<UpdatePromotionCommand, PromotionDto?>
{
    public async Task<PromotionDto?> Handle(UpdatePromotionCommand command, CancellationToken cancellationToken)
    {
        var promotion = await promotionRepository.GetByIdAsync(command.Id, cancellationToken);
        if (promotion is null)
        {
            return null;
        }

        ValidatePromotion(command.Promotion);

        promotion.Type = MapType(command.Promotion.Type);
        promotion.DiscountPercentage = command.Promotion.DiscountPercentage;
        promotion.ProductIds = command.Promotion.ProductIds.ToArray();
        promotion.StartsAtUtc = command.Promotion.StartsAtUtc;
        promotion.EndsAtUtc = command.Promotion.EndsAtUtc;
        promotion.RequiredPoints = command.Promotion.RequiredPoints;

        await promotionRepository.SaveChangesAsync(cancellationToken);

        return new PromotionDto(
            promotion.Id,
            MapType(promotion.Type),
            promotion.DiscountPercentage,
            promotion.ProductIds,
            promotion.StartsAtUtc,
            promotion.EndsAtUtc,
            promotion.RequiredPoints);
    }

    private static void ValidatePromotion(PromotionDto promotion)
    {
        if (promotion.DiscountPercentage <= 0 || promotion.DiscountPercentage > 100)
        {
            throw new ArgumentException("DiscountPercentage must be between 0 and 100.");
        }

        if (promotion.EndsAtUtc is not null && promotion.StartsAtUtc is not null && promotion.EndsAtUtc < promotion.StartsAtUtc)
        {
            throw new ArgumentException("EndsAtUtc cannot be earlier than StartsAtUtc.");
        }

        if (promotion.Type == PromotionTypeDto.ProductDiscount && promotion.ProductIds.Count == 0)
        {
            throw new ArgumentException("ProductDiscount promotion requires at least one ProductId.");
        }

        if (promotion.Type == PromotionTypeDto.LoyaltyPoints)
        {
            if (promotion.RequiredPoints is null || promotion.RequiredPoints <= 0)
            {
                throw new ArgumentException("LoyaltyPoints promotion requires RequiredPoints greater than 0.");
            }
        }
    }

    private static PromotionType MapType(PromotionTypeDto type)
    {
        return type switch
        {
            PromotionTypeDto.ProductDiscount => PromotionType.ProductDiscount,
            PromotionTypeDto.LoyaltyPoints => PromotionType.LoyaltyPoints,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown promotion type.")
        };
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
