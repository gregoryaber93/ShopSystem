using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;
using PromotionService.Application.Pipeline;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions;

public sealed class EvaluatePromotionsQueryHandler(
    PipelineRunner<EvaluationContext> pipelineRunner) : IQueryHandler<EvaluatePromotionsQuery, PromotionEvaluationResultDto>
{
    public async Task<PromotionEvaluationResultDto> Handle(EvaluatePromotionsQuery query, CancellationToken cancellationToken)
    {
        var context = new EvaluationContext { Request = query.Request };
        await pipelineRunner.RunAsync(context, cancellationToken);
        return context.Result ?? throw new InvalidOperationException("Evaluation pipeline did not produce a result.");
    }
}
