using PromotionService.Application.Pipeline;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;

public sealed class TotalCalculationFilter : IPipelineFilter<EvaluationContext>
{
    public async Task ExecuteAsync(EvaluationContext context, Func<Task> next, CancellationToken cancellationToken)
    {
        var totalDiscountPercentage = decimal.Min(100m, context.AppliedPromotions.Sum(a => a.DiscountPercentage));
        var discountAmount = decimal.Round(
            context.Request.Subtotal * (totalDiscountPercentage / 100m),
            2,
            MidpointRounding.AwayFromZero);
        var finalPrice = decimal.Max(
            0m,
            decimal.Round(context.Request.Subtotal - discountAmount, 2, MidpointRounding.AwayFromZero));

        context.Result = new PromotionEvaluationResultDto(
            context.Request.UserId,
            context.Request.Subtotal,
            totalDiscountPercentage,
            discountAmount,
            finalPrice,
            context.AppliedPromotions);

        await next();
    }
}
