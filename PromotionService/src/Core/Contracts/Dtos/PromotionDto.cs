namespace PromotionService.Contracts.Dtos;

public enum PromotionTypeDto
{
	ProductDiscount = 1,
	LoyaltyPoints = 2
}

public record PromotionDto(
	Guid Id,
	PromotionTypeDto Type,
	decimal DiscountPercentage,
	IReadOnlyCollection<Guid>? ProductIds,
	DateTime? StartsAtUtc,
	DateTime? EndsAtUtc,
	decimal? RequiredPoints);
