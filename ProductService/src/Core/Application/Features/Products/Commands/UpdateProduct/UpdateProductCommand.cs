using ProductService.Application.Abstractions.CQRS;
using ProductService.Contracts.Dtos;

namespace ProductService.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(Guid Id, ProductDto Product) : ICommand<ProductDto?>;
