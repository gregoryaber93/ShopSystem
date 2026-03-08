namespace PaymantService.Domain.Entities;

public sealed class PaymentEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ShopId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PLN";
    public string Method { get; set; } = "Card";
    public string Status { get; set; } = "Completed";
    public DateTime PaidAtUtc { get; set; }
}


