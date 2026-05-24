using AutoMapper;
using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Application.Abstractions.Security;
using PromotionService.Application.Features.Promotions;
using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Commands.AddPromotion;

public sealed class AddPromotionCommandHandler(
    IPromotionRepository promotionRepository,
    ICurrentUserService currentUserService,
    IMapper mapper) : ICommandHandler<AddPromotionCommand, PromotionDto>
{
    public async Task<PromotionDto> Handle(AddPromotionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Promotion;
        var normalized = PromotionValidation.NormalizeAndValidate(request);

        Guid? createdByUserId = currentUserService.IsInRole("Manager")
            ? currentUserService.GetUserIdOrThrow()
            : null;

        var promotion = new PromotionEntity
        {
            Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id,
            Type = normalized.Type,
            DiscountPercentage = normalized.DiscountPercentage,
            ProductIds = normalized.ProductIds,
            StartsAtUtc = normalized.StartsAtUtc,
            EndsAtUtc = normalized.EndsAtUtc,
            RequiredPoints = normalized.RequiredPoints,
            CreatedByUserId = createdByUserId
        };

        await promotionRepository.AddAsync(promotion, cancellationToken);
        await promotionRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<PromotionDto>(promotion);
    }
}
