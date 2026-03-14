using AuthenticationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Infrastructure.Persistence;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<AuthUserEntity> Users => Set<AuthUserEntity>();
    public DbSet<AuthRoleEntity> Roles => Set<AuthRoleEntity>();
    public DbSet<AuthUserRoleEntity> UserRoles => Set<AuthUserRoleEntity>();
    public DbSet<AuthOutboxMessageEntity> OutboxMessages => Set<AuthOutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthUserEntity>(entity =>
        {
            entity.ToTable("auth_users");
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Email)
                .HasMaxLength(320)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(512)
                .IsRequired();

            entity.HasIndex(user => user.Email)
                .IsUnique();
        });

        modelBuilder.Entity<AuthRoleEntity>(entity =>
        {
            entity.ToTable("auth_roles");
            entity.HasKey(role => role.Id);

            entity.Property(role => role.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(role => role.Name)
                .IsUnique();
        });

        modelBuilder.Entity<AuthUserRoleEntity>(entity =>
        {
            entity.ToTable("auth_user_roles");
            entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId });

            entity.HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuthOutboxMessageEntity>(entity =>
        {
            entity.ToTable("auth_outbox_messages");
            entity.HasKey(message => message.Id);

            entity.Property(message => message.EventType)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(message => message.PayloadJson)
                .IsRequired();

            entity.Property(message => message.Status)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(message => message.LastError)
                .HasMaxLength(2000);

            entity.HasIndex(message => new { message.Status, message.NextAttemptAtUtc });
        });
    }
}
