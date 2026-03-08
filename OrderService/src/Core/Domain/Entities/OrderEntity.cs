namespace OrderService.Domain.Entities;

public sealed class OrderEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ShopId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderedAtUtc { get; set; }
    public string Status { get; set; } = "Placed";
}
