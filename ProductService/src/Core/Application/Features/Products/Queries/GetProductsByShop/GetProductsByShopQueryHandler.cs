using ProductService.Application.Abstractions.CQRS;
using ProductService.Application.Abstractions.Persistence;
using ProductService.Contracts.Dtos;

namespace ProductService.Application.Features.Products.Queries.GetProductsByShop;

public sealed class GetProductsByShopQueryHandler(IProductRepository productRepository) : IQueryHandler<GetProductsByShopQuery, IReadOnlyCollection<ProductDto>>
{
    public async Task<IReadOnlyCollection<ProductDto>> Handle(GetProductsByShopQuery query, CancellationToken cancellationToken)
    {
        if (query.ShopId == Guid.Empty)
        {
            throw new ArgumentException("ShopId is required.");
        }

        var products = await productRepository.GetAllByShopIdAsync(query.ShopId, cancellationToken);

        return products
            .Select(product => new ProductDto(product.Id, product.Name, product.Type, product.Price, product.ShopId))
            .ToArray();
    }
}
