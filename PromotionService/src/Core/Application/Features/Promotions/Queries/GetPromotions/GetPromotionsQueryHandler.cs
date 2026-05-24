using AutoMapper;
using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Abstractions.Caching;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Features.Promotions.Queries.GetPromotions;
public sealed class GetPromotionsQueryHandler(
    IPromotionRepository promotionRepository,
    IPromotionCacheService promotionCacheService,
    IMapper mapper) : IQueryHandler<GetPromotionsQuery, IReadOnlyCollection<PromotionDto>>
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
            .Select(mapper.Map<PromotionDto>)
            .ToArray();

            await promotionCacheService.SetActivePromotionsAsync(result, TimeSpan.FromMinutes(5), cancellationToken);
            return result;
    }

}
