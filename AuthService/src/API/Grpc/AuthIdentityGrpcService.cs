using AuthService.Application.Abstractions.CQRS;
using AuthService.Application.Features.Authentication.Commands.DeleteIdentity;
using AuthService.Application.Features.Authentication.Commands.ProvisionIdentity;
using AuthService.Application.Features.Authentication.Commands.UpdateIdentity;
using AuthService.Contracts.Dtos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using ShopSystem.Contracts.Grpc.AuthIdentity;

namespace AuthService.Api.Grpc;

[Authorize(Roles = "Admin")]
public sealed class AuthIdentityGrpcService(
    ICommandHandler<ProvisionIdentityCommand, ProvisionedIdentityDto?> provisionIdentityCommandHandler,
    ICommandHandler<UpdateIdentityCommand, ProvisionedIdentityDto?> updateIdentityCommandHandler,
    ICommandHandler<DeleteIdentityCommand, bool> deleteIdentityCommandHandler) : AuthIdentityGrpc.AuthIdentityGrpcBase
{
    public override async Task<ProvisionUserResponse> ProvisionUser(ProvisionUserRequest request, ServerCallContext context)
    {
        var dto = new ProvisionIdentityRequestDto(request.Email, request.Password, request.Roles.ToArray());

        try
        {
            var result = await provisionIdentityCommandHandler.Handle(new ProvisionIdentityCommand(dto), context.CancellationToken);
            if (result is null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Uzytkownik o podanym emailu juz istnieje."));
            }

            var response = new ProvisionUserResponse
            {
                UserId = result.Id.ToString(),
                Email = result.Email
            };
            response.Roles.AddRange(result.Roles);

            return response;
        }
        catch (ArgumentException exception)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, exception.Message));
        }
    }

    public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId) || userId == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is invalid."));
        }

        var dto = new UpdateIdentityRequestDto(
            request.Email,
            request.Roles.ToArray(),
            request.HasPassword ? request.Password : null);

        try
        {
            var result = await updateIdentityCommandHandler.Handle(new UpdateIdentityCommand(userId, dto), context.CancellationToken);
            if (result is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Provisioned identity was not found."));
            }

            var response = new UpdateUserResponse
            {
                UserId = result.Id.ToString(),
                Email = result.Email
            };
            response.Roles.AddRange(result.Roles);

            return response;
        }
        catch (ArgumentException exception)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, exception.Message));
        }
    }

    public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId) || userId == Guid.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is invalid."));
        }

        var deleted = await deleteIdentityCommandHandler.Handle(new DeleteIdentityCommand(userId), context.CancellationToken);
        if (!deleted)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Provisioned identity was not found."));
        }

        return new DeleteUserResponse();
    }
}
