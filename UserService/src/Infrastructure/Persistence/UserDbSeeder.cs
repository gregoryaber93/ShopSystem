using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserService.Domain.Entities;
using UserService.Infrastructure.Security;

namespace UserService.Infrastructure.Persistence;

public static class UserDbSeeder
{
    public static async Task SeedDefaultsAsync(
        UserDbContext dbContext,
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

            dbContext.Roles.Add(new RoleEntity
            {
                Id = Guid.NewGuid(),
                Name = roleName
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var adminSeed = adminSeedOptions.Value;
        var adminEmail = adminSeed.Email.Trim().ToLowerInvariant();
        var adminExists = await dbContext.Users.AnyAsync(user => user.Email == adminEmail, cancellationToken);
        if (adminExists)
        {
            return;
        }

        var adminRole = await dbContext.Roles
            .FirstAsync(role => role.Name == "Admin", cancellationToken);

        var admin = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = adminEmail
        };

        admin.UserRoles.Add(new UserRoleEntity
        {
            UserId = admin.Id,
            RoleId = adminRole.Id,
            User = admin,
            Role = adminRole
        });

        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
