using LoggerService.Application.Abstractions;
using LoggerService.Infrastructure.Messaging;
using LoggerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoggerService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not found.");

        services.Configure<KafkaConsumerOptions>(configuration.GetSection("MessageBrokers:KafkaConsumers"));

        services.AddDbContext<LoggerDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ILogStore, DbLogStore>();
        services.AddHostedService<KafkaAuditConsumerWorker>();

        return services;
    }
}
