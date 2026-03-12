namespace PromotionService.Domain.Entities;

public class UserPromotionProfileEntity
{
    public Guid UserId { get; set; }
    public decimal LoyaltyPoints { get; set; }
    public int OrdersCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderAtUtc { get; set; }
}
