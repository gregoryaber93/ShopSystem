using System.Text.Json;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Application.Features.Orders.EventSourcing;
using OrderService.Contracts.Dtos;
using OrderService.Contracts.Messaging;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public sealed class OrderProjectionRebuilder(
    IOrderEventStore orderEventStore,
    IOrderRepository orderRepository) : IOrderProjectionRebuilder
{
    public async Task<OrderDto?> RebuildAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var aggregate = new OrderAggregateState();
        var snapshot = await orderEventStore.GetLatestSnapshotAsync(orderId, cancellationToken);
        var fromVersion = 0;

        if (snapshot is not null)
        {
            var snapshotModel = JsonSerializer.Deserialize<OrderAggregateState.Snapshot>(snapshot.Payload);
            if (snapshotModel is not null)
            {
                aggregate.ApplySnapshot(snapshotModel);
                fromVersion = snapshot.Version;
            }
        }

        var stream = await orderEventStore.LoadEventsAsync(orderId, fromVersion, cancellationToken);
        foreach (var @event in stream)
        {
            if (@event.EventType == "OrderPlaced" && @event.EventVersion == 1)
            {
                var payload = JsonSerializer.Deserialize<OrderPlacedEventV1>(@event.Payload);
                if (payload is not null)
                {
                    aggregate.Apply(payload, @event.Version);
                }
            }
        }

        if (aggregate.OrderId == Guid.Empty)
        {
            return null;
        }

        await orderRepository.UpsertAsync(new OrderEntity
        {
            Id = aggregate.OrderId,
            UserId = aggregate.UserId,
            ShopId = aggregate.ShopId,
            ProductId = aggregate.ProductId,
            Quantity = aggregate.Quantity,
            UnitPrice = aggregate.UnitPrice,
            TotalPrice = aggregate.TotalPrice,
            OrderedAtUtc = aggregate.OrderedAtUtc,
            Status = aggregate.Status
        }, cancellationToken);

        await orderRepository.SaveChangesAsync(cancellationToken);

        return new OrderDto(
            aggregate.OrderId,
            aggregate.UserId,
            aggregate.ShopId,
            aggregate.ProductId,
            aggregate.Quantity,
            aggregate.UnitPrice,
            aggregate.TotalPrice,
            aggregate.OrderedAtUtc,
            aggregate.Status);
    }
}
