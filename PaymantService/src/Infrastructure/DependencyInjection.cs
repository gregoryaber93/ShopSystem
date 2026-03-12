using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymantService.Application.Abstractions.Persistence;
using PaymantService.Infrastructure.Messaging;
using PaymantService.Infrastructure.Observability;
using PaymantService.Infrastructure.Outbox;
using PaymantService.Infrastructure.Persistence;

namespace PaymantService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not found.");

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(connectionString));

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "PaymantService:";
        });

        services.Configure<MessageBrokersOptions>(configuration.GetSection(MessageBrokersOptions.SectionName));
        services.Configure<PaymentOutboxOptions>(configuration.GetSection(PaymentOutboxOptions.SectionName));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentOutboxWriter, PaymentOutboxWriter>();
        services.AddScoped<IPaymentOutboxBrokerPublisher, PaymentOutboxBrokerPublisher>();
        services.AddHostedService<PaymentOutboxPublisherWorker>();

        var loggerServiceUrl = configuration[$"{LoggerServiceClientOptions.SectionName}:BaseUrl"] ?? "http://localhost:5300";
        services.AddHttpClient<ILoggerServiceClient, HttpLoggerServiceClient>(client =>
        {
            client.BaseAddress = new Uri(loggerServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}


