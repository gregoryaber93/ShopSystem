using Microsoft.EntityFrameworkCore;
using ShopService.Domain.Entities;

namespace ShopService.Infrastructure.Persistence;

public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    public DbSet<ShopEntity> Shops => Set<ShopEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShopEntity>(entity =>
        {
            entity.ToTable("shops");
            entity.HasKey(shop => shop.Id);

            entity.Property(shop => shop.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(shop => shop.Code)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(shop => shop.OwnerUserId)
                .IsRequired(false);

            entity.HasIndex(shop => shop.Code)
                .IsUnique();

            entity.HasIndex(shop => shop.OwnerUserId);
        });
    }
}
