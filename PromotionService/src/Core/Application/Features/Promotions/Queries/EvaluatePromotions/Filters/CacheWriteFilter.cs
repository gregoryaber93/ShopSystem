using PromotionService.Application.Abstractions.Caching;
using PromotionService.Application.Pipeline;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;

public sealed class CacheWriteFilter(IPromotionCacheService cacheService) : IPipelineFilter<EvaluationContext>
{
    public async Task ExecuteAsync(EvaluationContext context, Func<Task> next, CancellationToken cancellationToken)
    {
        if (context.Result is not null)
        {
            await cacheService.SetEligibilityAsync(
                context.Request.UserId,
                context.NormalizedProductIds,
                context.Request.Subtotal,
                context.Result,
                TimeSpan.FromMinutes(2),
                cancellationToken);
        }

        await next();
    }
}
