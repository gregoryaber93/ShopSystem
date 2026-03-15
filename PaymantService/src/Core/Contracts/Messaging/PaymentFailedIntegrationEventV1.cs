namespace PaymantService.Contracts.Messaging;

public sealed record PaymentFailedIntegrationEventV1(
    Guid EventId,
    DateTime OccurredOnUtc,
    Guid PaymentId,
    Guid UserId,
    Guid ShopId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    string Reason);
