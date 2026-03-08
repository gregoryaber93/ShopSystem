using ProductService.Application.Abstractions.CQRS;
using ProductService.Contracts.Dtos;

namespace ProductService.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDto?>;
