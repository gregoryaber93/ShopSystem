using Grpc.Net.ClientFactory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShopService.Application.Abstractions.Persistence;
using ShopService.Application.Abstractions.Messaging;
using ShopService.Contracts.Grpc.Orders;
using ShopService.Contracts.Grpc.Products;
using ShopService.Contracts.Grpc.Promotions;
using ShopService.Infrastructure.Integrations;
using ShopService.Infrastructure.Messaging;
using ShopService.Infrastructure.Persistence;
using ShopService.Infrastructure.Security;

namespace ShopService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not found.");

        services.AddDbContext<ShopDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IShopRepository, ShopRepository>();

        services.Configure<GrpcClientsOptions>(configuration.GetSection(GrpcClientsOptions.SectionName));
        services.Configure<MessageBrokersOptions>(configuration.GetSection(MessageBrokersOptions.SectionName));
        services.Configure<JwtRsaOptions>(configuration.GetSection(JwtRsaOptions.SectionName));

        services.AddSingleton<IEventPublisher, CompositeEventPublisher>();
        services.AddSingleton<RabbitMqEventPublisher>();
        services.AddSingleton<KafkaEventPublisher>();
        services.AddSingleton<IJwtTokenService, RsaJwtTokenService>();

        services.AddGrpcClients();

        return services;
    }

    private static IServiceCollection AddGrpcClients(this IServiceCollection services)
    {
        services.AddGrpcClient<ProductsGrpc.ProductsGrpcClient>((serviceProvider, options) =>
        {
            var grpcOptions = serviceProvider.GetRequiredService<IOptions<GrpcClientsOptions>>().Value;
            options.Address = new Uri(grpcOptions.Products);
        }).AddCallCredentials((context, metadata, serviceProvider) =>
        {
            var jwtTokenService = serviceProvider.GetRequiredService<IJwtTokenService>();
            var token = jwtTokenService.CreateServiceToken("shopservice-grpc-client");
            metadata.Add("Authorization", $"Bearer {token}");
            return Task.CompletedTask;
        });

        services.AddGrpcClient<PromotionsGrpc.PromotionsGrpcClient>((serviceProvider, options) =>
        {
            var grpcOptions = serviceProvider.GetRequiredService<IOptions<GrpcClientsOptions>>().Value;
            options.Address = new Uri(grpcOptions.Promotions);
        }).AddCallCredentials((context, metadata, serviceProvider) =>
        {
            var jwtTokenService = serviceProvider.GetRequiredService<IJwtTokenService>();
            var token = jwtTokenService.CreateServiceToken("shopservice-grpc-client");
            metadata.Add("Authorization", $"Bearer {token}");
            return Task.CompletedTask;
        });

        services.AddGrpcClient<OrdersGrpc.OrdersGrpcClient>((serviceProvider, options) =>
        {
            var grpcOptions = serviceProvider.GetRequiredService<IOptions<GrpcClientsOptions>>().Value;
            options.Address = new Uri(grpcOptions.Orders);
        }).AddCallCredentials((context, metadata, serviceProvider) =>
        {
            var jwtTokenService = serviceProvider.GetRequiredService<IJwtTokenService>();
            var token = jwtTokenService.CreateServiceToken("shopservice-grpc-client");
            metadata.Add("Authorization", $"Bearer {token}");
            return Task.CompletedTask;
        });

        return services;
    }
}