using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Abstractions.CQRS;
using ProductService.Application.Features.Products.Commands.AddProduct;
using ProductService.Application.Features.Products.Commands.DeleteProduct;
using ProductService.Application.Features.Products.Commands.UpdateProduct;
using ProductService.Application.Features.Products.Queries.GetProductById;
using ProductService.Application.Features.Products.Queries.GetProductsByShop;
using ProductService.Contracts.Dtos;

namespace ProductService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetProductsByShopQuery, IReadOnlyCollection<ProductDto>>, GetProductsByShopQueryHandler>();
        services.AddScoped<IQueryHandler<GetProductByIdQuery, ProductDto?>, GetProductByIdQueryHandler>();
        services.AddScoped<ICommandHandler<AddProductCommand, ProductDto>, AddProductCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProductCommand, ProductDto?>, UpdateProductCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteProductCommand, bool>, DeleteProductCommandHandler>();

        return services;
    }
}
