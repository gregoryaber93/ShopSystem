using PromotionService.Application.Pipeline;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;

public sealed class NormalizationFilter : IPipelineFilter<EvaluationContext>
{
    public async Task ExecuteAsync(EvaluationContext context, Func<Task> next, CancellationToken cancellationToken)
    {
        context.NormalizedProductIds = (context.Request.ProductIds ?? [])
            .Where(id => id != Guid.Empty)
            .OrderBy(id => id)
            .ToArray();

        context.ProductIdSet = context.NormalizedProductIds.ToHashSet();
        context.EvaluatedAtUtc = context.Request.EvaluatedAtUtc ?? DateTime.UtcNow;

        await next();
    }
}
