namespace OrderService.Application.Abstractions.Integrations;

public sealed record ProductSnapshot(
    Guid ProductId,
    Guid ShopId,
    decimal UnitPrice,
    string Name,
    string Type);
