namespace OrderService.Contracts.Dtos;

public sealed record PlaceOrderFromCartRequestDto(
    Guid ShopId,
    IReadOnlyCollection<CartOrderItemRequestDto> Items,
    bool AcceptPromotions = true);
