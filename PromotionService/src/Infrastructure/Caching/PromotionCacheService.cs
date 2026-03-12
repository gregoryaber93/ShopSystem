using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using PromotionService.Application.Abstractions.Caching;
using PromotionService.Contracts.Dtos;

namespace PromotionService.Infrastructure.Caching;

public sealed class PromotionCacheService(IDistributedCache cache) : IPromotionCacheService
{
    private const string ActivePromotionsKey = "promotions:active";

    public async Task<IReadOnlyCollection<PromotionDto>?> GetActivePromotionsAsync(CancellationToken cancellationToken)
    {
        var payload = await cache.GetStringAsync(ActivePromotionsKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        return JsonSerializer.Deserialize<IReadOnlyCollection<PromotionDto>>(payload);
    }

    public Task SetActivePromotionsAsync(IReadOnlyCollection<PromotionDto> promotions, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(promotions);
        return cache.SetStringAsync(ActivePromotionsKey, payload, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, cancellationToken);
    }

    public async Task<PromotionEvaluationResultDto?> GetEligibilityAsync(Guid userId, IReadOnlyCollection<Guid> productIds, decimal subtotal, CancellationToken cancellationToken)
    {
        var payload = await cache.GetStringAsync(EligibilityKey(userId, productIds, subtotal), cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        return JsonSerializer.Deserialize<PromotionEvaluationResultDto>(payload);
    }

    public Task SetEligibilityAsync(Guid userId, IReadOnlyCollection<Guid> productIds, decimal subtotal, PromotionEvaluationResultDto result, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(result);
        return cache.SetStringAsync(EligibilityKey(userId, productIds, subtotal), payload, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, cancellationToken);
    }

    private static string EligibilityKey(Guid userId, IReadOnlyCollection<Guid> productIds, decimal subtotal)
    {
        var productsSegment = string.Join(",", productIds.OrderBy(id => id).Select(id => id.ToString("N")));
        return $"promotions:eligibility:user:{userId:N}:products:{productsSegment}:subtotal:{subtotal:F2}";
    }
}
