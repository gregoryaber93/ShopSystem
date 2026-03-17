using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Application.Pipeline;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;

public sealed class DataLoadingFilter(
    IPromotionRepository promotionRepository,
    IUserPromotionProfileRepository userPromotionProfileRepository) : IPipelineFilter<EvaluationContext>
{
    public async Task ExecuteAsync(EvaluationContext context, Func<Task> next, CancellationToken cancellationToken)
    {
        var promotions = await promotionRepository.GetAllAsync(cancellationToken);
        var userProfile = await userPromotionProfileRepository.GetByUserIdAsync(context.Request.UserId, cancellationToken);

        context.Promotions = promotions.OrderBy(p => p.Id).ToArray();
        context.LoyaltyPoints = userProfile?.LoyaltyPoints ?? 0m;

        await next();
    }
}
