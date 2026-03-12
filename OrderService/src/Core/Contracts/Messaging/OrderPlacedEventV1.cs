namespace OrderService.Contracts.Messaging;

public sealed record OrderPlacedEventV1(
    Guid OrderId,
    Guid UserId,
    Guid ShopId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    DateTime OrderedAtUtc,
    string Status);
