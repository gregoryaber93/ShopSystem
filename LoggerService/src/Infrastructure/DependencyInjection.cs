using LoggerService.Application.Abstractions;
using LoggerService.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace LoggerService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ILogStore, InMemoryLogStore>();
        return services;
    }
}
