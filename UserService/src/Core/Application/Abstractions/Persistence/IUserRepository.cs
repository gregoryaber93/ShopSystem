using UserService.Domain.Entities;

namespace UserService.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserEntity?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RoleEntity>> GetOrCreateRolesAsync(IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken);
    Task AddUserAsync(UserEntity user, CancellationToken cancellationToken);
    void RemoveUserRole(UserRoleEntity userRole);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
