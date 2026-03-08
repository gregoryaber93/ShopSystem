using ProductService.Application.Abstractions.CQRS;
using ProductService.Contracts.Dtos;

namespace ProductService.Application.Features.Products.Commands.AddProduct;

public sealed record AddProductCommand(ProductDto Product) : ICommand<ProductDto>;
