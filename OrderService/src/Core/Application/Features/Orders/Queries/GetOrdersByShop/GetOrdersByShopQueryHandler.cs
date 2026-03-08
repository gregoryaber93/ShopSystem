using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Contracts.Dtos;

namespace OrderService.Application.Features.Orders.Queries.GetOrdersByShop;

public sealed class GetOrdersByShopQueryHandler(IOrderRepository orderRepository) : IQueryHandler<GetOrdersByShopQuery, IReadOnlyCollection<OrderDto>>
{
    public async Task<IReadOnlyCollection<OrderDto>> Handle(GetOrdersByShopQuery query, CancellationToken cancellationToken)
    {
        if (query.ShopId == Guid.Empty)
        {
            throw new ArgumentException("ShopId is required.");
        }

        var orders = await orderRepository.GetByShopIdAsync(query.ShopId, cancellationToken);
        return orders
            .Select(order => new OrderDto(
                order.Id,
                order.UserId,
                order.ShopId,
                order.ProductId,
                order.Quantity,
                order.UnitPrice,
                order.TotalPrice,
                order.OrderedAtUtc,
                order.Status))
            .ToList();
    }
}
