namespace OrderService.Contracts.Dtos;

public sealed record CartOrderLineDto(
    OrderDto Order,
    decimal LineSubtotal,
    decimal LineDiscountAmount,
    decimal LineFinalPrice);
