using PromotionService.Application.Pipeline;
using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;

public sealed class ProductDiscountFilter : IPipelineFilter<EvaluationContext>
{
    public async Task ExecuteAsync(EvaluationContext context, Func<Task> next, CancellationToken cancellationToken)
    {
        foreach (var promotion in context.Promotions.Where(p => p.Type == PromotionType.ProductDiscount))
        {
            if (promotion.ProductIds.Any(context.ProductIdSet.Contains))
            {
                context.AppliedPromotions.Add(new AppliedPromotionDto(
                    promotion.Id,
                    PromotionTypeDto.ProductDiscount,
                    promotion.DiscountPercentage,
                    "Product in cart matched promotion."));
            }
        }

        await next();
    }
}
