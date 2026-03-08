using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Contracts.Dtos;
using OrderService.Domain.Entities;

namespace OrderService.Application.Features.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandHandler(IOrderRepository orderRepository) : ICommandHandler<PlaceOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        ValidateRequest(command.UserId, command.Request);

        var normalizedUnitPrice = decimal.Round(command.Request.UnitPrice, 2, MidpointRounding.AwayFromZero);
        var totalPrice = decimal.Round(normalizedUnitPrice * command.Request.Quantity, 2, MidpointRounding.AwayFromZero);

        var order = new OrderEntity
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            ShopId = command.Request.ShopId,
            ProductId = command.Request.ProductId,
            Quantity = command.Request.Quantity,
            UnitPrice = normalizedUnitPrice,
            TotalPrice = totalPrice,
            OrderedAtUtc = DateTime.UtcNow,
            Status = "Placed"
        };

        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return ToDto(order);
    }

    private static void ValidateRequest(Guid userId, PlaceOrderRequestDto request)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.");
        }

        if (request.ShopId == Guid.Empty)
        {
            throw new ArgumentException("ShopId is required.");
        }

        if (request.ProductId == Guid.Empty)
        {
            throw new ArgumentException("ProductId is required.");
        }

        if (request.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");
        }

        if (request.UnitPrice < 0)
        {
            throw new ArgumentException("UnitPrice cannot be negative.");
        }
    }

    private static OrderDto ToDto(OrderEntity order)
    {
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
