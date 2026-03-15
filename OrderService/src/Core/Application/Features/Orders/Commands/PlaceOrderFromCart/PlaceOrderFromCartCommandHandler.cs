using System.Text.Json;
using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Abstractions.Integrations;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Application.Features.Orders.EventSourcing;
using OrderService.Contracts.Dtos;
using OrderService.Contracts.Messaging;
using OrderService.Domain.Entities;

namespace OrderService.Application.Features.Orders.Commands.PlaceOrderFromCart;

public sealed class PlaceOrderFromCartCommandHandler(
    IOrderRepository orderRepository,
    IOrderEventStore orderEventStore,
    IOrderOutboxWriter orderOutboxWriter,
    IProductCatalogGateway productCatalogGateway,
    IPromotionGateway promotionGateway) : ICommandHandler<PlaceOrderFromCartCommand, CartCheckoutResultDto>
{
    public async Task<CartCheckoutResultDto> Handle(PlaceOrderFromCartCommand command, CancellationToken cancellationToken)
    {
        ValidateRequest(command.UserId, command.Request);

        var normalizedItems = command.Request.Items
            .Where(item => item.ProductId != Guid.Empty && item.Quantity > 0)
            .GroupBy(item => item.ProductId)
            .Select(group => new CartOrderItemRequestDto(group.Key, group.Sum(x => x.Quantity)))
            .OrderBy(item => item.ProductId)
            .ToArray();

        var lines = new List<CartLineBuildModel>(normalizedItems.Length);
        foreach (var item in normalizedItems)
        {
            var product = await productCatalogGateway.GetProductByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
            {
                throw new ArgumentException($"Product '{item.ProductId}' does not exist.");
            }

            if (product.ShopId != command.Request.ShopId)
            {
                throw new ArgumentException($"Product '{item.ProductId}' does not belong to requested shop.");
            }

            var unitPrice = decimal.Round(product.UnitPrice, 2, MidpointRounding.AwayFromZero);
            var subtotal = decimal.Round(unitPrice * item.Quantity, 2, MidpointRounding.AwayFromZero);

            lines.Add(new CartLineBuildModel(product.ProductId, item.Quantity, unitPrice, subtotal));
        }

        var subtotalAll = decimal.Round(lines.Sum(line => line.Subtotal), 2, MidpointRounding.AwayFromZero);
        var evaluation = await promotionGateway.EvaluateAsync(
            command.UserId,
            subtotalAll,
            lines.Select(line => line.ProductId).ToArray(),
            cancellationToken);

        var promotionAccepted = command.Request.AcceptPromotions && evaluation.AppliedPromotions.Count > 0;
        var discountAmount = promotionAccepted
            ? decimal.Round(decimal.Min(subtotalAll, evaluation.DiscountAmount), 2, MidpointRounding.AwayFromZero)
            : 0m;

        var discountMap = BuildLineDiscounts(lines, subtotalAll, discountAmount);
        var createdOrderLines = new List<CartOrderLineDto>(lines.Count);

        foreach (var line in lines)
        {
            var lineDiscount = discountMap[line.ProductId];
            var lineFinal = decimal.Round(line.Subtotal - lineDiscount, 2, MidpointRounding.AwayFromZero);

            var order = new OrderEntity
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                ShopId = command.Request.ShopId,
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                TotalPrice = lineFinal,
                OrderedAtUtc = DateTime.UtcNow,
                Status = "Placed"
            };

            await orderRepository.AddAsync(order, cancellationToken);
            await AppendEventsAsync(order, cancellationToken);

            createdOrderLines.Add(new CartOrderLineDto(
                new OrderDto(
                    order.Id,
                    order.UserId,
                    order.ShopId,
                    order.ProductId,
                    order.Quantity,
                    order.UnitPrice,
                    order.TotalPrice,
                    order.OrderedAtUtc,
                    order.Status),
                line.Subtotal,
                lineDiscount,
                lineFinal));
        }

        await orderRepository.SaveChangesAsync(cancellationToken);

        var finalPrice = decimal.Round(subtotalAll - discountAmount, 2, MidpointRounding.AwayFromZero);
        var earnedPoints = CalculateEarnedPoints(finalPrice);
        var benefits = promotionAccepted
            ? evaluation.AppliedPromotions
                .Select(item => new AppliedBenefitDto(item.PromotionId, item.Type, item.DiscountPercentage, item.Reason))
                .ToArray()
            : [];

        return new CartCheckoutResultDto(
            command.UserId,
            command.Request.ShopId,
            subtotalAll,
            discountAmount,
            finalPrice,
            earnedPoints,
            promotionAccepted,
            benefits,
            createdOrderLines);
    }

    private async Task AppendEventsAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        var placedEvent = new OrderPlacedEventV1(
            order.Id,
            order.UserId,
            order.ShopId,
            order.ProductId,
            order.Quantity,
            order.UnitPrice,
            order.TotalPrice,
            order.OrderedAtUtc,
            order.Status);

        await orderEventStore.AppendAsync(
            order.Id,
            "OrderPlaced",
            1,
            JsonSerializer.Serialize(placedEvent),
            order.OrderedAtUtc,
            expectedVersion: 0,
            cancellationToken);

        var state = new OrderAggregateState();
        state.Apply(placedEvent, 1);

        await orderEventStore.SaveSnapshotAsync(new OrderSnapshotEntity
        {
            AggregateId = order.Id,
            Version = state.Version,
            Payload = JsonSerializer.Serialize(state.ToSnapshot()),
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

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
    }

    private static Dictionary<Guid, decimal> BuildLineDiscounts(IReadOnlyCollection<CartLineBuildModel> lines, decimal subtotalAll, decimal discountAmount)
    {
        var discounts = new Dictionary<Guid, decimal>(lines.Count);
        if (discountAmount <= 0 || subtotalAll <= 0)
        {
            foreach (var line in lines)
            {
                discounts[line.ProductId] = 0m;
            }

            return discounts;
        }

        var ordered = lines.OrderBy(line => line.ProductId).ToArray();
        var remaining = discountAmount;

        for (var i = 0; i < ordered.Length; i++)
        {
            var line = ordered[i];
            decimal lineDiscount;

            if (i == ordered.Length - 1)
            {
                lineDiscount = remaining;
            }
            else
            {
                lineDiscount = decimal.Round(discountAmount * (line.Subtotal / subtotalAll), 2, MidpointRounding.AwayFromZero);
                if (lineDiscount > remaining)
                {
                    lineDiscount = remaining;
                }
            }

            if (lineDiscount > line.Subtotal)
            {
                lineDiscount = line.Subtotal;
            }

            discounts[line.ProductId] = lineDiscount;
            remaining = decimal.Round(remaining - lineDiscount, 2, MidpointRounding.AwayFromZero);
        }

        return discounts;
    }

    private static int CalculateEarnedPoints(decimal finalPrice)
    {
        if (finalPrice <= 0)
        {
            return 0;
        }

        // 1 point per each 10 currency units paid.
        return (int)decimal.Floor(finalPrice / 10m);
    }

    private static void ValidateRequest(Guid userId, PlaceOrderFromCartRequestDto request)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.");
        }

        if (request.ShopId == Guid.Empty)
        {
            throw new ArgumentException("ShopId is required.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException("At least one cart item is required.");
        }

        if (request.Items.Any(item => item.ProductId == Guid.Empty))
        {
            throw new ArgumentException("Cart item ProductId is required.");
        }

        if (request.Items.Any(item => item.Quantity <= 0))
        {
            throw new ArgumentException("Cart item quantity must be greater than zero.");
        }
    }

    private sealed record CartLineBuildModel(
        Guid ProductId,
        int Quantity,
        decimal UnitPrice,
        decimal Subtotal);
}
