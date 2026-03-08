using DashboardService.Application.Abstractions;
using DashboardService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DashboardService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardAppService>();
        return services;
    }
}


