using Microsoft.Extensions.DependencyInjection;
using ShopService.Application.Abstractions.CQRS;
using ShopService.Application.Features.Health.Commands.Ping;
using ShopService.Application.Features.Health.Queries.GetHealth;
using ShopService.Application.Features.Shops.Commands.AddShop;
using ShopService.Application.Features.Shops.Commands.DeleteShop;
using ShopService.Application.Features.Shops.Commands.UpdateShop;
using ShopService.Application.Features.Shops.Queries.GetShops;
using ShopService.Contracts.Dtos;

namespace ShopService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetHealthQuery, HealthStatusResponse>, GetHealthQueryHandler>();
        services.AddScoped<ICommandHandler<PingCommand, bool>, PingCommandHandler>();

        services.AddScoped<IQueryHandler<GetShopsQuery, IReadOnlyCollection<ShopDto>>, GetShopsQueryHandler>();
        services.AddScoped<ICommandHandler<AddShopCommand, ShopDto>, AddShopCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateShopCommand, ShopDto?>, UpdateShopCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteShopCommand, bool>, DeleteShopCommandHandler>();

        return services;
    }
}