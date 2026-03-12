using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Contracts.Dtos;

namespace OrderService.Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(IOrderRepository orderRepository) : IQueryHandler<GetOrderByIdQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.OrderId == Guid.Empty)
        {
            throw new ArgumentException("OrderId is required.");
        }

        var order = await orderRepository.GetByIdAsync(query.OrderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        return new OrderDto(
            order.Id,
            order.UserId,
            order.ShopId,
            order.ProductId,
            order.Quantity,
            order.UnitPrice,
            order.TotalPrice,
            order.OrderedAtUtc,
            order.Status);
    }
}
