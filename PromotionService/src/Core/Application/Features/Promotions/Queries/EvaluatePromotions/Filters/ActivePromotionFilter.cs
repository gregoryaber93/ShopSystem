using PromotionService.Application.Pipeline;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;

public sealed class ActivePromotionFilter : IPipelineFilter<EvaluationContext>
{
    public async Task ExecuteAsync(EvaluationContext context, Func<Task> next, CancellationToken cancellationToken)
    {
        context.Promotions = context.Promotions
            .Where(p => IsActive(p, context.EvaluatedAtUtc))
            .ToArray();

        await next();
    }

    private static bool IsActive(PromotionEntity promotion, DateTime evaluatedAtUtc)
    {
        var started = promotion.StartsAtUtc is null || promotion.StartsAtUtc.Value <= evaluatedAtUtc;
        var notEnded = promotion.EndsAtUtc is null || promotion.EndsAtUtc.Value >= evaluatedAtUtc;
        return started && notEnded;
    }
}
