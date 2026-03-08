using ProductService.Application.Abstractions.CQRS;
using ProductService.Contracts.Dtos;

namespace ProductService.Application.Features.Products.Queries.GetProductsByShop;

public sealed record GetProductsByShopQuery(Guid ShopId) : IQuery<IReadOnlyCollection<ProductDto>>;
