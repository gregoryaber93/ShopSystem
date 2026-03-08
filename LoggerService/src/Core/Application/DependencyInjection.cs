using LoggerService.Application.Abstractions;
using LoggerService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LoggerService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILoggingService, LoggingService>();
        return services;
    }
}
