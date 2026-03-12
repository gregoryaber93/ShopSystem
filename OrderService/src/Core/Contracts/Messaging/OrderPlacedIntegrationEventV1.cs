namespace OrderService.Contracts.Messaging;

public sealed record OrderPlacedIntegrationEventV1(
    Guid EventId,
    DateTime OccurredOnUtc,
    Guid OrderId,
    Guid UserId,
    Guid ShopId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Status);
