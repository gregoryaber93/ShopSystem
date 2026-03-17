using PromotionService.Contracts.Dtos;
using PromotionService.Domain.Entities;

namespace PromotionService.Application.Features.Promotions.Queries.EvaluatePromotions.Filters;

public sealed class EvaluationContext
{
    public required PromotionEvaluationRequestDto Request { get; init; }

    // Set by NormalizationFilter
    public Guid[] NormalizedProductIds { get; set; } = [];
    public HashSet<Guid> ProductIdSet { get; set; } = [];
    public DateTime EvaluatedAtUtc { get; set; }

    // Set by DataLoadingFilter
    public IReadOnlyList<PromotionEntity> Promotions { get; set; } = [];
    public decimal LoyaltyPoints { get; set; }

    // Built by discount filters
    public List<AppliedPromotionDto> AppliedPromotions { get; } = [];

    // Set by TotalCalculationFilter (or CacheReadFilter on cache hit)
    public PromotionEvaluationResultDto? Result { get; set; }
}
