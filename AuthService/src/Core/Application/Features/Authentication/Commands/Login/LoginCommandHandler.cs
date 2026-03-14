using AuthService.Application.Abstractions.CQRS;
using AuthService.Application.Abstractions.Persistence;
using AuthService.Application.Abstractions.Security;
using AuthService.Contracts.Dtos;

namespace AuthService.Application.Features.Authentication.Commands.Login;

public sealed class LoginCommandHandler(
    IAuthUserRepository authUserRepository,
    IPasswordHasherService passwordHasherService,
    IJwtTokenService jwtTokenService) : ICommandHandler<LoginCommand, AuthResponseDto?>
{
    public async Task<AuthResponseDto?> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim().ToLowerInvariant();
        var user = await authUserRepository.GetByEmailWithRolesAsync(email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (!passwordHasherService.VerifyPassword(command.Request.Password, user.PasswordHash))
        {
            return null;
        }

        var roles = user.UserRoles.Select(userRole => userRole.Role.Name).ToArray();
        var (token, expiresAtUtc) = jwtTokenService.CreateUserToken(user.Id, user.Email, roles);

        return new AuthResponseDto(token, expiresAtUtc, user.Id, user.Email, roles);
    }
}
