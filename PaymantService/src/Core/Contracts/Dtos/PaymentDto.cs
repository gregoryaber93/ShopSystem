namespace PaymantService.Contracts.Dtos;

public sealed record PaymentDto(
    Guid Id,
    Guid UserId,
    Guid ShopId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    DateTime PaidAtUtc);


