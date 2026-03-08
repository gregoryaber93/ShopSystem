using DashboardService.Application.Abstractions;
using DashboardService.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace DashboardService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDashboardStore, InMemoryDashboardStore>();
        return services;
    }
}


