using PromotionService.Contracts.Dtos;

namespace PromotionService.Application.Abstractions.Caching;

public interface IPromotionCacheService
{
    Task<IReadOnlyCollection<PromotionDto>?> GetActivePromotionsAsync(CancellationToken cancellationToken);
    Task SetActivePromotionsAsync(IReadOnlyCollection<PromotionDto> promotions, TimeSpan ttl, CancellationToken cancellationToken);
    Task<PromotionEvaluationResultDto?> GetEligibilityAsync(Guid userId, IReadOnlyCollection<Guid> productIds, decimal subtotal, CancellationToken cancellationToken);
    Task SetEligibilityAsync(Guid userId, IReadOnlyCollection<Guid> productIds, decimal subtotal, PromotionEvaluationResultDto result, TimeSpan ttl, CancellationToken cancellationToken);
}
