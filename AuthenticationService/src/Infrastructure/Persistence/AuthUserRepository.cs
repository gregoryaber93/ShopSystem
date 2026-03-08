using AuthenticationService.Application.Abstractions.Persistence;
using AuthenticationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Infrastructure.Persistence;

public sealed class AuthUserRepository(AuthDbContext dbContext) : IAuthUserRepository
{
    public Task<AuthUserEntity?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuthRoleEntity>> GetOrCreateRolesAsync(IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken)
    {
        var normalized = roleNames
            .Select(role => role.Trim())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var roles = await dbContext.Roles
            .Where(role => normalized.Contains(role.Name))
            .ToListAsync(cancellationToken);

        var existing = roles.Select(role => role.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in normalized)
        {
            if (existing.Contains(roleName))
            {
                continue;
            }

            var newRole = new AuthRoleEntity
            {
                Id = Guid.NewGuid(),
                Name = roleName
            };

            dbContext.Roles.Add(newRole);
            roles.Add(newRole);
        }

        return roles;
    }

    public Task AddUserAsync(AuthUserEntity user, CancellationToken cancellationToken)
    {
        return dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
