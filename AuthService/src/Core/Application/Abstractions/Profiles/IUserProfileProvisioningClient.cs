namespace AuthService.Application.Abstractions.Profiles;

public interface IUserProfileProvisioningClient
{
    Task<bool> CreateOrUpdateProfileAsync(Guid userId, string email, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);
}
