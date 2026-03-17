using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Application.Abstractions.Security;
using PromotionService.Application.Features.Promotions;
using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Commands.UpdatePromotion;

public sealed class UpdatePromotionCommandHandler(
    IPromotionRepository promotionRepository,
    ICurrentUserService currentUserService) : ICommandHandler<UpdatePromotionCommand, PromotionDto?>
{
    public async Task<PromotionDto?> Handle(UpdatePromotionCommand command, CancellationToken cancellationToken)
    {
        var promotion = await promotionRepository.GetByIdAsync(command.Id, cancellationToken);
        if (promotion is null)
        {
            return null;
        }

        if (currentUserService.IsInRole("Manager"))
        {
            var currentUserId = currentUserService.GetUserIdOrThrow();
            if (promotion.CreatedByUserId != currentUserId)
            {
                throw new UnauthorizedAccessException("Manager can only update their own promotions.");
            }
        }

        var normalized = PromotionValidation.NormalizeAndValidate(command.Promotion);

        promotion.Type = normalized.Type;
        promotion.DiscountPercentage = normalized.DiscountPercentage;
        promotion.ProductIds = normalized.ProductIds;
        promotion.StartsAtUtc = normalized.StartsAtUtc;
        promotion.EndsAtUtc = normalized.EndsAtUtc;
        promotion.RequiredPoints = normalized.RequiredPoints;

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
