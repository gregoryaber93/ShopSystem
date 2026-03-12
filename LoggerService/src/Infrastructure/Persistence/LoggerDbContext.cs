using LoggerService.Domain;
using Microsoft.EntityFrameworkCore;

namespace LoggerService.Infrastructure.Persistence;

public sealed class LoggerDbContext(DbContextOptions<LoggerDbContext> options) : DbContext(options)
{
    public DbSet<LogEntry> LogEntries => Set<LogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.ToTable("log_entries");
            entity.HasKey(log => log.Id);

            entity.Property(log => log.Level)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(log => log.Message)
                .HasColumnType("text")
                .IsRequired();

            entity.Property(log => log.Source)
                .HasMaxLength(200);

            entity.Property(log => log.CorrelationId)
                .HasMaxLength(100);

            entity.Property(log => log.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(log => log.CorrelationId);
            entity.HasIndex(log => log.Source);
            entity.HasIndex(log => log.CreatedAtUtc);
        });
    }
}
