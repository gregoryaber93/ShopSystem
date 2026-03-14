using System.Text.Json;
using AuthService.Application.Abstractions.Outbox;
using AuthService.Application.Abstractions.Security;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Outbox;
using AuthService.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Persistence;

public static class AuthDbSeeder
{
    public static async Task SeedDefaultsAsync(
        AuthDbContext dbContext,
        IPasswordHasherService passwordHasherService,
        IOptions<AdminSeedOptions> adminSeedOptions,
        CancellationToken cancellationToken)
    {
        var requiredRoles = new[] { "Admin", "Manager", "User" };
        var existingRoles = await dbContext.Roles
            .Select(role => role.Name)
            .ToListAsync(cancellationToken);

        foreach (var roleName in requiredRoles)
        {
            if (existingRoles.Any(role => string.Equals(role, roleName, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            dbContext.Roles.Add(new AuthRoleEntity
            {
                Id = Guid.NewGuid(),
                Name = roleName
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var adminSeed = adminSeedOptions.Value;
        var adminEmail = adminSeed.Email.Trim().ToLowerInvariant();
        var admin = await dbContext.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Email == adminEmail, cancellationToken);

        if (admin is null)
        {
            var adminRole = await dbContext.Roles
                .FirstAsync(role => role.Name == "Admin", cancellationToken);

            admin = new AuthUserEntity
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                PasswordHash = passwordHasherService.HashPassword(adminSeed.Password)
            };

            admin.UserRoles.Add(new AuthUserRoleEntity
            {
                UserId = admin.Id,
                RoleId = adminRole.Id,
                User = admin,
                Role = adminRole
            });

            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await EnsureAdminUserCreatedOutboxMessageAsync(dbContext, admin, cancellationToken);
    }

    private static async Task EnsureAdminUserCreatedOutboxMessageAsync(
        AuthDbContext dbContext,
        AuthUserEntity admin,
        CancellationToken cancellationToken)
    {
        var userIdMarker = $"\"UserId\":\"{admin.Id}\"";
        var alreadyQueued = await dbContext.OutboxMessages
            .AnyAsync(
                message => message.EventType == AuthOutboxConstants.UserCreatedEventType
                    && message.PayloadJson.Contains(userIdMarker)
                    && (message.Status == AuthOutboxStatus.Pending
                        || message.Status == AuthOutboxStatus.FailedRetryable
                        || message.Status == AuthOutboxStatus.Completed),
                cancellationToken);

        if (alreadyQueued)
        {
            return;
        }

        var roles = admin.UserRoles
            .Select(userRole => userRole.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (roles.Length == 0)
        {
            roles = ["Admin"];
        }

        var payload = new OutboxUserCreatedPayload(admin.Id, admin.Email, roles);
        var message = new AuthOutboxMessageEntity
        {
            Id = Guid.NewGuid(),
            EventType = AuthOutboxConstants.UserCreatedEventType,
            PayloadJson = JsonSerializer.Serialize(payload),
            Status = AuthOutboxStatus.Pending,
            RetryCount = 0,
            CreatedAtUtc = DateTime.UtcNow,
            NextAttemptAtUtc = DateTime.UtcNow
        };

        dbContext.OutboxMessages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}