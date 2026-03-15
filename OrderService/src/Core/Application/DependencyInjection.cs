using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Features.Orders.Commands.PlaceOrder;
using OrderService.Application.Features.Orders.Commands.PlaceOrderFromCart;
using OrderService.Application.Features.Orders.Queries.GetOrderById;
using OrderService.Application.Features.Orders.Queries.GetMyOrders;
using OrderService.Application.Features.Orders.Queries.GetOrdersByShop;
using OrderService.Application.Features.Orders.Queries.ReplayOrderProjection;
using OrderService.Contracts.Dtos;

namespace OrderService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<PlaceOrderCommand, OrderDto>, PlaceOrderCommandHandler>();
        services.AddScoped<ICommandHandler<PlaceOrderFromCartCommand, CartCheckoutResultDto>, PlaceOrderFromCartCommandHandler>();
        services.AddScoped<IQueryHandler<GetOrderByIdQuery, OrderDto?>, GetOrderByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetMyOrdersQuery, IReadOnlyCollection<OrderDto>>, GetMyOrdersQueryHandler>();
        services.AddScoped<IQueryHandler<GetOrdersByShopQuery, IReadOnlyCollection<OrderDto>>, GetOrdersByShopQueryHandler>();
        services.AddScoped<IQueryHandler<ReplayOrderProjectionQuery, OrderDto?>, ReplayOrderProjectionQueryHandler>();

        return services;
    }
}
