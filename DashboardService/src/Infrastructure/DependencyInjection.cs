using DashboardService.Application.Abstractions;
using DashboardService.Infrastructure.Messaging;
using DashboardService.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DashboardService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaConsumerOptions>(configuration.GetSection("MessageBrokers:KafkaConsumers"));
        services.AddSingleton<IDashboardStore, InMemoryDashboardStore>();
        services.AddHostedService<DashboardKafkaProjectionWorker>();
        return services;
    }
}


