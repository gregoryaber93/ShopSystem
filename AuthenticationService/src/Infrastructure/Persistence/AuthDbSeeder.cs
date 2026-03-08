using AuthenticationService.Domain.Entities;

namespace AuthenticationService.Infrastructure.Persistence;

public static class AuthDbSeeder
{
    public static void SeedDefaults(AuthDbContext dbContext)
    {
        var requiredRoles = new[] { "Admin", "Manager", "User" };

        var existingRoles = dbContext.Roles
            .Select(role => role.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in requiredRoles)
        {
            if (existingRoles.Contains(roleName))
            {
                continue;
            }

            dbContext.Roles.Add(new AuthRoleEntity
            {
                Id = Guid.NewGuid(),
                Name = roleName
            });
        }

        dbContext.SaveChanges();
    }
}
