using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Abstractions.Persistence;

public interface ILoyaltyProjectionRebuilder
{
    Task<UserPromotionProfileDto?> RebuildAsync(Guid userId, CancellationToken cancellationToken);
}
