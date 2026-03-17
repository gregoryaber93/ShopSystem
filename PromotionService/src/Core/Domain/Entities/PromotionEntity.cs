namespace PromotionService.Domain.Entities;

public enum PromotionType
{
    ProductDiscount = 1,
    LoyaltyPoints = 2
}

public class PromotionEntity
{
    public Guid Id { get; set; }
    public PromotionType Type { get; set; }
    public decimal DiscountPercentage { get; set; }
    public Guid[] ProductIds { get; set; } = [];
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public decimal? RequiredPoints { get; set; }
    public Guid? CreatedByUserId { get; set; }
}
