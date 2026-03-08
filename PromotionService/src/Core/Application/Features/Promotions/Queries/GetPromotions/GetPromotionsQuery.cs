using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Features.Promotions.Queries.GetPromotions;

public sealed record GetPromotionsQuery : IQuery<IReadOnlyCollection<PromotionDto>>;
