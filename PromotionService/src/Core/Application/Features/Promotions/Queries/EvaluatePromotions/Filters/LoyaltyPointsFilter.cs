using PromotionService.Application.Pipeline;
using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;

public sealed class LoyaltyPointsFilter : IPipelineFilter<EvaluationContext>
{
    public async Task ExecuteAsync(EvaluationContext context, Func<Task> next, CancellationToken cancellationToken)
    {
        foreach (var promotion in context.Promotions.Where(p => p.Type == PromotionType.LoyaltyPoints))
        {
            if (promotion.RequiredPoints is not null && context.LoyaltyPoints >= promotion.RequiredPoints.Value)
            {
                context.AppliedPromotions.Add(new AppliedPromotionDto(
                    promotion.Id,
                    PromotionTypeDto.LoyaltyPoints,
                    promotion.DiscountPercentage,
                    "User loyalty points threshold met."));
            }
        }

        await next();
    }
}
