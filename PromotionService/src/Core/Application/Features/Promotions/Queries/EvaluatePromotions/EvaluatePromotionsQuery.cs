using PromotionService.Application.Abstractions.CQRS;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions;

public sealed record EvaluatePromotionsQuery(PromotionEvaluationRequestDto Request) : IQuery<PromotionEvaluationResultDto>;
