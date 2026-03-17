using Microsoft.EntityFrameworkCore;
using UserService.Application.Abstractions.Persistence;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence;

public sealed class UserRepository(UserDbContext dbContext) : IUserRepository
{
    public Task<UserEntity?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public Task<UserEntity?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken)
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

    public async Task<IReadOnlyCollection<RoleEntity>> GetOrCreateRolesAsync(IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken)
    {
        var normalized = roleNames
            .Select(role => role.Trim())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var roles = await dbContext.Roles
            .Where(role => normalized.Contains(role.Name))
            .ToListAsync(cancellationToken);

        var existing = roles
            .Select(role => role.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in normalized)
        {
            if (existing.Contains(roleName))
            {
                continue;
            }

            var role = new RoleEntity
            {
                Id = Guid.NewGuid(),
                Name = roleName
            };

            dbContext.Roles.Add(role);
            roles.Add(role);
        }

        return roles;
    }

    public Task AddUserAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }

    public void RemoveUserRole(UserRoleEntity userRole)
    {
        userRole.User.UserRoles.Remove(userRole);
        dbContext.UserRoles.Remove(userRole);
    }

    public void RemoveUser(UserEntity user)
    {
        dbContext.Users.Remove(user);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
