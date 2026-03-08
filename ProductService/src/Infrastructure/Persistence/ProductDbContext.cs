using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Persistence;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("products", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_products_price_nonnegative", "\"Price\" >= 0");
                tableBuilder.HasCheckConstraint("CK_products_name_not_empty", "btrim(\"Name\") <> ''");
                tableBuilder.HasCheckConstraint("CK_products_type_not_empty", "btrim(\"Type\") <> ''");
            });
            entity.HasKey(product => product.Id);

            entity.Property(product => product.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(product => product.Type)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(product => product.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(product => product.ShopId)
                .IsRequired();

            entity.HasIndex(product => product.ShopId);
        });
    }
}
