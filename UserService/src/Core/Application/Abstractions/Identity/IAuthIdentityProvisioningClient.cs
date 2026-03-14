namespace UserService.Application.Abstractions.Identity;

public interface IAuthIdentityProvisioningClient
{
    Task<ProvisionedAuthIdentity?> ProvisionUserAsync(string email, string password, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);
    Task RollbackProvisionAsync(Guid userId, CancellationToken cancellationToken);
}