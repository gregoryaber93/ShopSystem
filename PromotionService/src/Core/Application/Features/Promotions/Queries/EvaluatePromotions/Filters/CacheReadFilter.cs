using PromotionService.Application.Abstractions.Caching;
using PromotionService.Application.Pipeline;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;

public sealed class CacheReadFilter(IPromotionCacheService cacheService) : IPipelineFilter<EvaluationContext>
{
    public async Task ExecuteAsync(EvaluationContext context, Func<Task> next, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetEligibilityAsync(
            context.Request.UserId,
            context.NormalizedProductIds,
            context.Request.Subtotal,
            cancellationToken);

        if (cached is not null)
        {
            context.Result = cached;
            return; // short-circuit: skip remaining filters
        }

        await next();
    }
}
