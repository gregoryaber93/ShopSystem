using Microsoft.EntityFrameworkCore;
using PromotionService.Domain.Entities;

namespace PromotionService.Infrastructure.Persistence;

public class PromotionDbContext(DbContextOptions<PromotionDbContext> options) : DbContext(options)
{
    public DbSet<PromotionEntity> Promotions => Set<PromotionEntity>();
    public DbSet<UserPromotionProfileEntity> UserPromotionProfiles => Set<UserPromotionProfileEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PromotionEntity>(entity =>
        {
            entity.ToTable("promotions", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_promotions_discount_range", "\"DiscountPercentage\" > 0 AND \"DiscountPercentage\" <= 100");
                tableBuilder.HasCheckConstraint("CK_promotions_date_range", "\"StartsAtUtc\" IS NULL OR \"EndsAtUtc\" IS NULL OR \"EndsAtUtc\" >= \"StartsAtUtc\"");
                tableBuilder.HasCheckConstraint("CK_promotions_required_points", "\"Type\" <> 2 OR (\"RequiredPoints\" IS NOT NULL AND \"RequiredPoints\" > 0)");
            });
            entity.HasKey(promotion => promotion.Id);

            entity.Property(promotion => promotion.Type)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(promotion => promotion.DiscountPercentage)
                .HasPrecision(5, 2)
                .IsRequired();

            entity.Property(promotion => promotion.ProductIds)
                .HasColumnType("uuid[]")
                .IsRequired();

            entity.Property(promotion => promotion.StartsAtUtc)
                .HasColumnType("timestamp with time zone");

            entity.Property(promotion => promotion.EndsAtUtc)
                .HasColumnType("timestamp with time zone");

            entity.Property(promotion => promotion.RequiredPoints)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<UserPromotionProfileEntity>(entity =>
        {
            entity.ToTable("user_promotion_profiles", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_user_promotion_profiles_points_nonnegative", "\"LoyaltyPoints\" >= 0");
                tableBuilder.HasCheckConstraint("CK_user_promotion_profiles_orders_nonnegative", "\"OrdersCount\" >= 0");
                tableBuilder.HasCheckConstraint("CK_user_promotion_profiles_total_spent_nonnegative", "\"TotalSpent\" >= 0");
            });

            entity.HasKey(profile => profile.UserId);

            entity.Property(profile => profile.LoyaltyPoints)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(profile => profile.OrdersCount)
                .IsRequired();

            entity.Property(profile => profile.TotalSpent)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(profile => profile.LastOrderAtUtc)
                .HasColumnType("timestamp with time zone");
        });
    }
}
