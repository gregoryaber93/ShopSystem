using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Features.Orders.Commands.PlaceOrder;
using OrderService.Application.Features.Orders.Queries.GetMyOrders;
using OrderService.Application.Features.Orders.Queries.GetOrdersByShop;
using OrderService.Contracts.Dtos;

namespace OrderService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<PlaceOrderCommand, OrderDto>, PlaceOrderCommandHandler>();
        services.AddScoped<IQueryHandler<GetMyOrdersQuery, IReadOnlyCollection<OrderDto>>, GetMyOrdersQueryHandler>();
        services.AddScoped<IQueryHandler<GetOrdersByShopQuery, IReadOnlyCollection<OrderDto>>, GetOrdersByShopQueryHandler>();

        return services;
    }
}
