using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Features.Promotions.Queries.ReplayUserLoyaltyProjection;

public sealed record ReplayUserLoyaltyProjectionQuery(Guid UserId) : IQuery<UserPromotionProfileDto?>;
