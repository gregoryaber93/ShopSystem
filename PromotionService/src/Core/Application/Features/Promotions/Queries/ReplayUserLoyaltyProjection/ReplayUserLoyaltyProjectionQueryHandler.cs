using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Features.Promotions.Queries.ReplayUserLoyaltyProjection;

public sealed class ReplayUserLoyaltyProjectionQueryHandler(
    ILoyaltyProjectionRebuilder loyaltyProjectionRebuilder) : IQueryHandler<ReplayUserLoyaltyProjectionQuery, UserPromotionProfileDto?>
{
    public Task<UserPromotionProfileDto?> Handle(ReplayUserLoyaltyProjectionQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.");
        }

        return loyaltyProjectionRebuilder.RebuildAsync(query.UserId, cancellationToken);
    }
}
