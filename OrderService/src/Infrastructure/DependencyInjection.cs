using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderService.Application.Abstractions.Integrations;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Observability;
using OrderService.Infrastructure.Outbox;
using OrderService.Infrastructure.ProductCatalog;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Security;
using Polly;
using Polly.Extensions.Http;
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

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "OrderService:";
        });

        services.Configure<JwtRsaOptions>(configuration.GetSection(JwtRsaOptions.SectionName));
        services.Configure<MessageBrokersOptions>(configuration.GetSection(MessageBrokersOptions.SectionName));
        services.Configure<OrderOutboxOptions>(configuration.GetSection(OrderOutboxOptions.SectionName));
        services.AddSingleton<IJwtTokenService, RsaJwtTokenService>();

        var grpcAddress = configuration[$"{ProductCatalogGrpcOptions.SectionName}:Address"];
        if (string.IsNullOrWhiteSpace(grpcAddress))
        {
            throw new InvalidOperationException($"Configuration '{ProductCatalogGrpcOptions.SectionName}:Address' is required.");
        }

        services.AddGrpcClient<ProductsGrpc.ProductsGrpcClient>(options =>
        {
            options.Address = new Uri(grpcAddress);
        })
        .AddCallCredentials((context, metadata, serviceProvider) =>
        {
            var jwtTokenService = serviceProvider.GetRequiredService<IJwtTokenService>();
            var token = jwtTokenService.CreateServiceToken("orderservice-grpc-client");
            metadata.Add("Authorization", $"Bearer {token}");
            return Task.CompletedTask;
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy())
        .AddPolicyHandler(GetTimeoutPolicy());

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderEventStore, OrderEventStore>();
        services.AddScoped<IOrderProjectionRebuilder, OrderProjectionRebuilder>();
        services.AddScoped<IProductCatalogGateway, ProductCatalogGrpcGateway>();
        services.AddScoped<IOrderOutboxWriter, OrderOutboxWriter>();
        services.AddScoped<IOrderOutboxBrokerPublisher, OrderOutboxBrokerPublisher>();
        services.AddHostedService<OrderOutboxPublisherWorker>();

        var loggerServiceUrl = configuration[$"{LoggerServiceClientOptions.SectionName}:BaseUrl"] ?? "http://localhost:5300";
        services.AddHttpClient<ILoggerServiceClient, HttpLoggerServiceClient>(client =>
        {
            client.BaseAddress = new Uri(loggerServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => (int)response.StatusCode == 429)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));
    }
}
