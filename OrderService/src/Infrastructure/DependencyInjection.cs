using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Abstractions.Integrations;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Infrastructure.ProductCatalog;
using OrderService.Infrastructure.Persistence;
using ShopSystem.Contracts.Grpc.Products;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not found.");

        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(connectionString));

        var grpcAddress = configuration[$"{ProductCatalogGrpcOptions.SectionName}:Address"];
        if (string.IsNullOrWhiteSpace(grpcAddress))
        {
            throw new InvalidOperationException($"Configuration '{ProductCatalogGrpcOptions.SectionName}:Address' is required.");
        }

        services.AddGrpcClient<ProductsGrpc.ProductsGrpcClient>(options =>
        {
            options.Address = new Uri(grpcAddress);
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductCatalogGateway, ProductCatalogGrpcGateway>();

        return services;
    }
}
