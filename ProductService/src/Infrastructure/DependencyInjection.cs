using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Abstractions.Persistence;
using ProductService.Application.Abstractions.Caching;
using ProductService.Application.Abstractions.Security;
using ProductService.Application.Abstractions.Shops;
using ProductService.Infrastructure.Caching;
using ProductService.Infrastructure.Observability;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Security;
using ProductService.Infrastructure.Shops;

namespace ProductService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not found.");

        services.AddDbContext<ProductDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductCacheService, ProductCacheService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

        var shopServiceUrl = configuration[$"{ShopServiceClientOptions.SectionName}:BaseUrl"] ?? "http://localhost:5292";
        services.AddHttpClient<IShopOwnershipClient, HttpShopOwnershipClient>(client =>
        {
            client.BaseAddress = new Uri(shopServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "ProductService:";
        });

        var loggerServiceUrl = configuration[$"{LoggerServiceClientOptions.SectionName}:BaseUrl"] ?? "http://localhost:5300";
        services.AddHttpClient<ILoggerServiceClient, HttpLoggerServiceClient>(client =>
        {
            client.BaseAddress = new Uri(loggerServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
