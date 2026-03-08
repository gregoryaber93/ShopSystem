using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Infrastructure.Persistence;

namespace PromotionService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not found.");

        services.AddDbContext<PromotionDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPromotionRepository, PromotionRepository>();

        return services;
    }
}
