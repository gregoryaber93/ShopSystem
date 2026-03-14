using Grpc.Core;
using ShopSystem.Contracts.Grpc.UserProfiles;
using UserService.Application.Abstractions.CQRS;
using UserService.Application.Features.Users.Commands.CreateOrUpdateUserProfile;
using UserService.Contracts.Dtos;
using UserService.Infrastructure.Security;

namespace UserService.Api.Grpc;

public sealed class UserProfilesGrpcService(
    ICommandHandler<CreateOrUpdateUserProfileCommand, UserDto?> createOrUpdateUserProfileCommandHandler,
    IConfiguration configuration) : UserProfilesGrpc.UserProfilesGrpcBase
{
    public override async Task<CreateOrUpdateProfileResponse> CreateOrUpdateProfile(CreateOrUpdateProfileRequest request, ServerCallContext context)
    {
        var configuredKey = configuration[$"{InternalApiOptions.SectionName}:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey) || !string.Equals(configuredKey, request.InternalApiKey, StringComparison.Ordinal))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid internal API key."));
        }

        if (!Guid.TryParse(request.UserId, out var userId) || userId == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is invalid."));
        }

        var dto = new CreateOrUpdateUserProfileRequestDto(userId, request.Email, request.Roles.ToArray());

        try
        {
            var result = await createOrUpdateUserProfileCommandHandler.Handle(new CreateOrUpdateUserProfileCommand(dto), context.CancellationToken);
            if (result is null)
            {
                return new CreateOrUpdateProfileResponse
                {
                    Success = false,
                    Conflict = true,
                    Error = "Email is already linked to another profile."
                };
            }

            return new CreateOrUpdateProfileResponse
            {
                Success = true,
                Conflict = false
            };
        }
        catch (ArgumentException exception)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, exception.Message));
        }
    }
}
