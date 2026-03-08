using ProductService.Application.Abstractions.CQRS;
using ProductService.Application.Abstractions.Persistence;
using ProductService.Contracts.Dtos;

namespace ProductService.Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductRepository productRepository) : IQueryHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.ProductId == Guid.Empty)
        {
            throw new ArgumentException("ProductId is required.");
        }

        var product = await productRepository.GetByIdAsync(query.ProductId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        return new ProductDto(product.Id, product.Name, product.Type, product.Price, product.ShopId);
    }
}
