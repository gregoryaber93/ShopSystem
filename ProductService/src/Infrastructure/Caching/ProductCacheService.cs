using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ProductService.Application.Abstractions.Caching;
using ProductService.Contracts.Dtos;

namespace ProductService.Infrastructure.Caching;

public sealed class ProductCacheService(IDistributedCache cache) : IProductCacheService
{
    private static string Key(Guid shopId) => $"products:shop:{shopId:N}";

    public async Task<IReadOnlyCollection<ProductDto>?> GetProductsByShopAsync(Guid shopId, CancellationToken cancellationToken)
    {
        var payload = await cache.GetStringAsync(Key(shopId), cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        return JsonSerializer.Deserialize<IReadOnlyCollection<ProductDto>>(payload);
    }

    public Task SetProductsByShopAsync(Guid shopId, IReadOnlyCollection<ProductDto> products, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(products);
        return cache.SetStringAsync(Key(shopId), payload, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, cancellationToken);
    }

    public Task RemoveProductsByShopAsync(Guid shopId, CancellationToken cancellationToken)
    {
        return cache.RemoveAsync(Key(shopId), cancellationToken);
    }
}
