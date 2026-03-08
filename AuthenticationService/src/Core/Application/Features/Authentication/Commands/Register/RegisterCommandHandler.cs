using AuthenticationService.Application.Abstractions.CQRS;
using AuthenticationService.Application.Abstractions.Persistence;
using AuthenticationService.Application.Abstractions.Security;
using AuthenticationService.Contracts.Dtos;
using AuthenticationService.Domain.Entities;

namespace AuthenticationService.Application.Features.Authentication.Commands.Register;

public sealed class RegisterCommandHandler(
    IAuthUserRepository authUserRepository,
    IPasswordHasherService passwordHasherService,
    IJwtTokenService jwtTokenService) : ICommandHandler<RegisterCommand, AuthResponseDto?>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Manager",
        "User"
    };

    public async Task<AuthResponseDto?> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim().ToLowerInvariant();
        if (await authUserRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return null;
        }

        var roles = command.Request.Roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (roles.Length == 0)
        {
            roles = ["User"];
        }

        var invalidRoles = roles.Where(role => !AllowedRoles.Contains(role)).ToArray();
        if (invalidRoles.Length > 0)
        {
            throw new ArgumentException($"Nieobslugiwane role: {string.Join(", ", invalidRoles)}");
        }

        var roleEntities = await authUserRepository.GetOrCreateRolesAsync(roles, cancellationToken);

        var user = new AuthUserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasherService.HashPassword(command.Request.Password)
        };

        foreach (var role in roleEntities)
        {
            user.UserRoles.Add(new AuthUserRoleEntity
            {
                UserId = user.Id,
                RoleId = role.Id,
                User = user,
                Role = role
            });
        }

        await authUserRepository.AddUserAsync(user, cancellationToken);
        await authUserRepository.SaveChangesAsync(cancellationToken);

        var assignedRoles = user.UserRoles.Select(userRole => userRole.Role.Name).ToArray();
        var (token, expiresAtUtc) = jwtTokenService.CreateUserToken(user.Id, user.Email, assignedRoles);

        return new AuthResponseDto(token, expiresAtUtc, user.Id, user.Email, assignedRoles);
    }
}
