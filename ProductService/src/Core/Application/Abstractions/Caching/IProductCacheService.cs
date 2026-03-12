using ProductService.Contracts.Dtos;

namespace ProductService.Application.Abstractions.Caching;

public interface IProductCacheService
{
    Task<IReadOnlyCollection<ProductDto>?> GetProductsByShopAsync(Guid shopId, CancellationToken cancellationToken);
    Task SetProductsByShopAsync(Guid shopId, IReadOnlyCollection<ProductDto> products, TimeSpan ttl, CancellationToken cancellationToken);
    Task RemoveProductsByShopAsync(Guid shopId, CancellationToken cancellationToken);
}
