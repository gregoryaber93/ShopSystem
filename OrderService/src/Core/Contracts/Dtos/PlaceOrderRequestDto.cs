namespace OrderService.Contracts.Dtos;

public sealed record PlaceOrderRequestDto(
    Guid ShopId,
    Guid ProductId,
    int Quantity,
    decimal? UnitPrice = null);
