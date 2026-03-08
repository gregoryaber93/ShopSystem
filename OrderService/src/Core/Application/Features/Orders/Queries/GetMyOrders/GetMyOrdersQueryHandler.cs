using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Contracts.Dtos;

namespace OrderService.Application.Features.Orders.Queries.GetMyOrders;

public sealed class GetMyOrdersQueryHandler(IOrderRepository orderRepository) : IQueryHandler<GetMyOrdersQuery, IReadOnlyCollection<OrderDto>>
{
    public async Task<IReadOnlyCollection<OrderDto>> Handle(GetMyOrdersQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.");
        }

        var orders = await orderRepository.GetByUserIdAsync(query.UserId, cancellationToken);
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
