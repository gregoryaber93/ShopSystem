using AuthService.Application.Abstractions.Profiles;
using Microsoft.Extensions.Options;
using ShopSystem.Contracts.Grpc.UserProfiles;

namespace AuthService.Infrastructure.Profiles;

internal sealed class UserProfileProvisioningClient(
    UserProfilesGrpc.UserProfilesGrpcClient grpcClient,
    IOptions<UserServiceClientOptions> options) : IUserProfileProvisioningClient
{
    public async Task<bool> CreateOrUpdateProfileAsync(Guid userId, string email, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        var configuredOptions = options.Value;

        var response = await grpcClient.CreateOrUpdateProfileAsync(
            new CreateOrUpdateProfileRequest
            {
                UserId = userId.ToString(),
                Email = email,
                Roles = { roles },
                InternalApiKey = configuredOptions.InternalApiKey ?? string.Empty
            },
            cancellationToken: cancellationToken);

        if (response.Conflict)
        {
            return false;
        }

        if (!response.Success)
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.Error)
                ? "Nieznany blad podczas synchronizacji profilu przez gRPC."
                : response.Error);
        }

        return true;
    }
}
