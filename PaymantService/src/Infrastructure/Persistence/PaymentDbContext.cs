using Microsoft.EntityFrameworkCore;
using PaymantService.Domain.Entities;

namespace PaymantService.Infrastructure.Persistence;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
    public DbSet<PaymentOutboxMessageEntity> PaymentOutboxMessages => Set<PaymentOutboxMessageEntity>();
    public DbSet<PaymentProcessedEventEntity> PaymentProcessedEvents => Set<PaymentProcessedEventEntity>();

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

        modelBuilder.Entity<PaymentOutboxMessageEntity>(entity =>
        {
            entity.ToTable("payment_outbox_messages", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_payment_outbox_event_type_not_empty", "btrim(\"EventType\") <> ''");
                tableBuilder.HasCheckConstraint("CK_payment_outbox_payload_not_empty", "btrim(\"Payload\") <> ''");
            });

            entity.HasKey(message => message.Id);
            entity.HasIndex(message => message.EventId).IsUnique();
            entity.HasIndex(message => message.ProcessedAtUtc);
            entity.HasIndex(message => message.NextRetryAtUtc);

            entity.Property(message => message.EventType)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(message => message.Payload)
                .HasColumnType("text")
                .IsRequired();

            entity.Property(message => message.PartitionKey)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(message => message.LastError)
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<PaymentProcessedEventEntity>(entity =>
        {
            entity.ToTable("payment_processed_events");
            entity.HasKey(processed => processed.EventId);
            entity.Property(processed => processed.ProcessedAtUtc).IsRequired();
        });
    }
}


