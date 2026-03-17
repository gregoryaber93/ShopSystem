using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Application.Abstractions.Caching;
using PromotionService.Application.Abstractions.Security;
using PromotionService.Infrastructure.Caching;
using PromotionService.Infrastructure.Observability;
using PromotionService.Infrastructure.Persistence;
using PromotionService.Infrastructure.Security;

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
        services.AddScoped<IPromotionCacheService, PromotionCacheService>();
        services.AddScoped<IUserPromotionProfileRepository, UserPromotionProfileRepository>();
        services.AddScoped<ILoyaltyEventStore, LoyaltyEventStore>();
        services.AddScoped<ILoyaltyProjectionRebuilder, LoyaltyProjectionRebuilder>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "PromotionService:";
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
