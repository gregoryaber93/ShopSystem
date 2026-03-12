using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Application.Features.Promotions;
using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Commands.AddPromotion;

public sealed class AddPromotionCommandHandler(IPromotionRepository promotionRepository) : ICommandHandler<AddPromotionCommand, PromotionDto>
{
    public async Task<PromotionDto> Handle(AddPromotionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Promotion;
        var normalized = PromotionValidation.NormalizeAndValidate(request);

        var promotion = new PromotionEntity
        {
            Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id,
            Type = normalized.Type,
            DiscountPercentage = normalized.DiscountPercentage,
            ProductIds = normalized.ProductIds,
            StartsAtUtc = normalized.StartsAtUtc,
            EndsAtUtc = normalized.EndsAtUtc,
            RequiredPoints = normalized.RequiredPoints
        };

        await promotionRepository.AddAsync(promotion, cancellationToken);
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
