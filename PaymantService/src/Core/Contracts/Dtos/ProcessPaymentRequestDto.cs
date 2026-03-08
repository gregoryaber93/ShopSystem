namespace PaymantService.Contracts.Dtos;

public sealed record ProcessPaymentRequestDto(
    Guid ShopId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Method);


