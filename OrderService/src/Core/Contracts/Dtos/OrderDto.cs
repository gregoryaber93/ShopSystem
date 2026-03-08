namespace OrderService.Contracts.Dtos;

public sealed record OrderDto(
    Guid Id,
    Guid UserId,
    Guid ShopId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    DateTime OrderedAtUtc,
    string Status);
