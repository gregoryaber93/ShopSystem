using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Abstractions.Integrations;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Contracts.Dtos;
using OrderService.Contracts.Messaging;
using OrderService.Domain.Entities;
using System.Text.Json;

namespace OrderService.Application.Features.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    IOrderOutboxWriter orderOutboxWriter,
    IProductCatalogGateway productCatalogGateway) : ICommandHandler<PlaceOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        ValidateRequest(command.UserId, command.Request);

        var product = await productCatalogGateway.GetProductByIdAsync(command.Request.ProductId, cancellationToken);
        if (product is null)
        {
            throw new ArgumentException("Product does not exist.");
        }

        if (product.ShopId != command.Request.ShopId)
        {
            throw new ArgumentException("Product does not belong to requested shop.");
        }

        var normalizedUnitPrice = decimal.Round(product.UnitPrice, 2, MidpointRounding.AwayFromZero);
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

        var integrationEvent = new OrderPlacedIntegrationEventV1(
            EventId: Guid.NewGuid(),
            OccurredOnUtc: order.OrderedAtUtc,
            OrderId: order.Id,
            UserId: order.UserId,
            ShopId: order.ShopId,
            ProductId: order.ProductId,
            Quantity: order.Quantity,
            UnitPrice: order.UnitPrice,
            TotalPrice: order.TotalPrice,
            Status: order.Status);

        await orderOutboxWriter.EnqueueAsync(
            integrationEvent.EventId,
            "OrderPlaced",
            JsonSerializer.Serialize(integrationEvent),
            order.UserId.ToString("N"),
            integrationEvent.OccurredOnUtc,
            cancellationToken);

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
