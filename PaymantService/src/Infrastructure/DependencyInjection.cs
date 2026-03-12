using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymantService.Application.Abstractions.Persistence;
using PaymantService.Infrastructure.Messaging;
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

        services.Configure<MessageBrokersOptions>(configuration.GetSection(MessageBrokersOptions.SectionName));
        services.Configure<PaymentOutboxOptions>(configuration.GetSection(PaymentOutboxOptions.SectionName));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentOutboxWriter, PaymentOutboxWriter>();
        services.AddScoped<IPaymentOutboxBrokerPublisher, PaymentOutboxBrokerPublisher>();
        services.AddHostedService<PaymentOutboxPublisherWorker>();

        return services;
    }
}


