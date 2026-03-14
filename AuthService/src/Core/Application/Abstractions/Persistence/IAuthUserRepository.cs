using AuthenticationService.Domain.Entities;

namespace AuthenticationService.Application.Abstractions.Persistence;

public interface IAuthUserRepository
{
    Task<AuthUserEntity?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<AuthUserEntity?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AuthRoleEntity>> GetOrCreateRolesAsync(IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken);
    Task AddUserAsync(AuthUserEntity user, CancellationToken cancellationToken);
    void RemoveUser(AuthUserEntity user);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
