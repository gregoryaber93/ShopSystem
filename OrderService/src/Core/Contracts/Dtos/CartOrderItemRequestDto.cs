namespace OrderService.Contracts.Dtos;

public sealed record CartOrderItemRequestDto(
    Guid ProductId,
    int Quantity);
