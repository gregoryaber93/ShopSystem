using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("orders", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_orders_quantity_positive", "\"Quantity\" > 0");
                tableBuilder.HasCheckConstraint("CK_orders_unit_price_nonnegative", "\"UnitPrice\" >= 0");
                tableBuilder.HasCheckConstraint("CK_orders_total_price_nonnegative", "\"TotalPrice\" >= 0");
                tableBuilder.HasCheckConstraint("CK_orders_status_not_empty", "btrim(\"Status\") <> ''");
            });

            entity.HasKey(order => order.Id);

            entity.Property(order => order.UserId)
                .IsRequired();

            entity.Property(order => order.ShopId)
                .IsRequired();

            entity.Property(order => order.ProductId)
                .IsRequired();

            entity.Property(order => order.Quantity)
                .IsRequired();

            entity.Property(order => order.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(order => order.TotalPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(order => order.OrderedAtUtc)
                .IsRequired();

            entity.Property(order => order.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(order => order.UserId);
            entity.HasIndex(order => order.ShopId);
            entity.HasIndex(order => order.ProductId);
            entity.HasIndex(order => order.OrderedAtUtc);
        });
    }
}
