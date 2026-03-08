using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymantService.Application.Abstractions.Persistence;
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

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        return services;
    }
}


