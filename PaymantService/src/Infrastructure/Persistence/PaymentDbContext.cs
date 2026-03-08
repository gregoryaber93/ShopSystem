using Microsoft.EntityFrameworkCore;
using PaymantService.Domain.Entities;

namespace PaymantService.Infrastructure.Persistence;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.ToTable("payments", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_payments_amount_positive", "\"Amount\" > 0");
                tableBuilder.HasCheckConstraint("CK_payments_currency_not_empty", "btrim(\"Currency\") <> ''");
                tableBuilder.HasCheckConstraint("CK_payments_method_not_empty", "btrim(\"Method\") <> ''");
                tableBuilder.HasCheckConstraint("CK_payments_status_not_empty", "btrim(\"Status\") <> ''");
            });

            entity.HasKey(payment => payment.Id);

            entity.Property(payment => payment.UserId)
                .IsRequired();

            entity.Property(payment => payment.ShopId)
                .IsRequired();

            entity.Property(payment => payment.OrderId)
                .IsRequired();

            entity.Property(payment => payment.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(payment => payment.Currency)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(payment => payment.Method)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(payment => payment.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(payment => payment.PaidAtUtc)
                .IsRequired();

            entity.HasIndex(payment => payment.UserId);
            entity.HasIndex(payment => payment.ShopId);
            entity.HasIndex(payment => payment.OrderId);
            entity.HasIndex(payment => payment.PaidAtUtc);
        });
    }
}


