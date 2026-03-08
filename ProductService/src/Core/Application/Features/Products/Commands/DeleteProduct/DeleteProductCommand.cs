using ProductService.Application.Abstractions.CQRS;

namespace ProductService.Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand<bool>;
