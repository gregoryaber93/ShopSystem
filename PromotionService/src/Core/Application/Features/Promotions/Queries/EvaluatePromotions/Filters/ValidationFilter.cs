using PromotionService.Application.Pipeline;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;

public sealed class ValidationFilter : IPipelineFilter<EvaluationContext>
{
    public async Task ExecuteAsync(EvaluationContext context, Func<Task> next, CancellationToken cancellationToken)
    {
        if (context.Request.UserId == Guid.Empty)
            throw new ArgumentException("UserId is required.");

        if (context.Request.Subtotal < 0)
            throw new ArgumentException("Subtotal cannot be negative.");

        await next();
    }
}
