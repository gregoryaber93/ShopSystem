using Grpc.Core;
using Microsoft.AspNetCore.Http;
using ShopSystem.Contracts.Grpc.AuthIdentity;
using UserService.Application.Abstractions.Identity;

namespace UserService.Infrastructure.Identity;

internal sealed class AuthIdentityProvisioningClient(
    AuthIdentityGrpc.AuthIdentityGrpcClient grpcClient,
    IHttpContextAccessor httpContextAccessor) : IAuthIdentityProvisioningClient
{
    public async Task<ProvisionedAuthIdentity?> ProvisionUserAsync(string email, string password, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        try
        {
            var response = await grpcClient.ProvisionUserAsync(
                new ProvisionUserRequest
                {
                    Email = email,
                    Password = password,
                    Roles = { roles }
                },
                headers: CreateHeaders(),
                cancellationToken: cancellationToken);

            return new ProvisionedAuthIdentity(Guid.Parse(response.UserId), response.Email, response.Roles.ToArray());
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.AlreadyExists)
        {
            return null;
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.InvalidArgument)
        {
            throw new ArgumentException(exception.Status.Detail);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.PermissionDenied || exception.StatusCode == StatusCode.Unauthenticated)
        {
            throw new UnauthorizedAccessException("Biezacy uzytkownik nie ma uprawnien do provisioningu identity w AuthService.");
        }
    }

    public async Task<ProvisionedAuthIdentity?> UpdateUserAsync(
        Guid userId,
        string email,
        IReadOnlyCollection<string> roles,
        string? password,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new UpdateUserRequest
            {
                UserId = userId.ToString(),
                Email = email
            };
            request.Roles.AddRange(roles);

            if (!string.IsNullOrWhiteSpace(password))
            {
                request.Password = password;
            }

            var response = await grpcClient.UpdateUserAsync(
                request,
                headers: CreateHeaders(),
                cancellationToken: cancellationToken);

            return new ProvisionedAuthIdentity(Guid.Parse(response.UserId), response.Email, response.Roles.ToArray());
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.AlreadyExists)
        {
            throw new InvalidOperationException(exception.Status.Detail);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.InvalidArgument)
        {
            throw new ArgumentException(exception.Status.Detail);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.PermissionDenied || exception.StatusCode == StatusCode.Unauthenticated)
        {
            throw new UnauthorizedAccessException("Biezacy uzytkownik nie ma uprawnien do aktualizacji identity w AuthService.");
        }
    }

    public async Task RollbackProvisionAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await grpcClient.DeleteUserAsync(
                new DeleteUserRequest { UserId = userId.ToString() },
                headers: CreateHeaders(),
                cancellationToken: cancellationToken);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
        {
            return;
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.PermissionDenied || exception.StatusCode == StatusCode.Unauthenticated)
        {
            throw new UnauthorizedAccessException("Biezacy uzytkownik nie ma uprawnien do usuwania identity w AuthService.");
        }
    }

    private Metadata CreateHeaders()
    {
        var authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            throw new InvalidOperationException("Brak naglowka Authorization do przekazania do AuthService.");
        }

        return new Metadata
        {
            { "authorization", authorizationHeader }
        };
    }
}