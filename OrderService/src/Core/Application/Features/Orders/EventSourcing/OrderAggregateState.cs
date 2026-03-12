using OrderService.Contracts.Messaging;

namespace OrderService.Application.Features.Orders.EventSourcing;

public sealed class OrderAggregateState
{
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    public DateTime OrderedAtUtc { get; private set; }
    public string Status { get; private set; } = "Placed";
    public int Version { get; private set; }

    public void Apply(OrderPlacedEventV1 @event, int version)
    {
        OrderId = @event.OrderId;
        UserId = @event.UserId;
        ShopId = @event.ShopId;
        ProductId = @event.ProductId;
        Quantity = @event.Quantity;
        UnitPrice = @event.UnitPrice;
        TotalPrice = @event.TotalPrice;
        OrderedAtUtc = @event.OrderedAtUtc;
        Status = @event.Status;
        Version = version;
    }

    public Snapshot ToSnapshot()
    {
        return new Snapshot(
            OrderId,
            UserId,
            ShopId,
            ProductId,
            Quantity,
            UnitPrice,
            TotalPrice,
            OrderedAtUtc,
            Status,
            Version);
    }

    public void ApplySnapshot(Snapshot snapshot)
    {
        OrderId = snapshot.OrderId;
        UserId = snapshot.UserId;
        ShopId = snapshot.ShopId;
        ProductId = snapshot.ProductId;
        Quantity = snapshot.Quantity;
        UnitPrice = snapshot.UnitPrice;
        TotalPrice = snapshot.TotalPrice;
        OrderedAtUtc = snapshot.OrderedAtUtc;
        Status = snapshot.Status;
        Version = snapshot.Version;
    }

    public sealed record Snapshot(
        Guid OrderId,
        Guid UserId,
        Guid ShopId,
        Guid ProductId,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice,
        DateTime OrderedAtUtc,
        string Status,
        int Version);
}
