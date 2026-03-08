using UserService.Domain.Entities;

namespace UserService.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RoleEntity>> GetOrCreateRolesAsync(IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken);
    Task AddUserAsync(UserEntity user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
