using ProductService.Application.Abstractions.Caching;
using ProductService.Application.Abstractions.CQRS;
using ProductService.Application.Abstractions.Persistence;
using ProductService.Contracts.Dtos;

namespace ProductService.Application.Features.Products.Queries.GetProductsByShop;

public sealed class GetProductsByShopCachedQueryHandler(
    IProductRepository productRepository,
    IProductCacheService productCacheService) : IQueryHandler<GetProductsByShopQuery, IReadOnlyCollection<ProductDto>>
{
    public async Task<IReadOnlyCollection<ProductDto>> Handle(GetProductsByShopQuery query, CancellationToken cancellationToken)
    {
        if (query.ShopId == Guid.Empty)
        {
            throw new ArgumentException("ShopId is required.");
        }

        var cached = await productCacheService.GetProductsByShopAsync(query.ShopId, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var products = await productRepository.GetAllByShopIdAsync(query.ShopId, cancellationToken);
        var result = products
            .Select(product => new ProductDto(product.Id, product.Name, product.Type, product.Price, product.ShopId))
            .ToArray();

        await productCacheService.SetProductsByShopAsync(query.ShopId, result, TimeSpan.FromMinutes(5), cancellationToken);
        return result;
    }
}
