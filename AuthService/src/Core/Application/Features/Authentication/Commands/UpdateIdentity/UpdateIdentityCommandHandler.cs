using AuthService.Application.Abstractions.CQRS;
using AuthService.Application.Abstractions.Persistence;
using AuthService.Application.Abstractions.Security;
using AuthService.Contracts.Dtos;
using AuthService.Domain.Entities;

namespace AuthService.Application.Features.Authentication.Commands.UpdateIdentity;

public sealed class UpdateIdentityCommandHandler(
    IAuthUserRepository authUserRepository,
    IPasswordHasherService passwordHasherService) : ICommandHandler<UpdateIdentityCommand, ProvisionedIdentityDto?>
{
    private static readonly HashSet<string> SupportedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Manager",
        "User"
    };

    public async Task<ProvisionedIdentityDto?> Handle(UpdateIdentityCommand command, CancellationToken cancellationToken)
    {
        if (command.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.");
        }

        var user = await authUserRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var email = command.Request.Email.Trim().ToLowerInvariant();
        var existingByEmail = await authUserRepository.GetByEmailWithRolesAsync(email, cancellationToken);
        if (existingByEmail is not null && existingByEmail.Id != user.Id)
        {
            throw new InvalidOperationException("Uzytkownik o podanym emailu juz istnieje.");
        }

        var roles = (command.Request.Roles ?? [])
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (roles.Length == 0)
        {
            roles = ["User"];
        }

        var invalidRoles = roles.Where(role => !SupportedRoles.Contains(role)).ToArray();
        if (invalidRoles.Length > 0)
        {
            throw new ArgumentException($"Nieobslugiwane role: {string.Join(", ", invalidRoles)}");
        }

        user.Email = email;
        if (!string.IsNullOrWhiteSpace(command.Request.Password))
        {
            user.PasswordHash = passwordHasherService.HashPassword(command.Request.Password);
        }

        var existingRoles = user.UserRoles.ToArray();
        foreach (var userRole in existingRoles)
        {
            authUserRepository.RemoveUserRole(userRole);
        }

        var roleEntities = await authUserRepository.GetOrCreateRolesAsync(roles, cancellationToken);
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

        await authUserRepository.SaveChangesAsync(cancellationToken);
        var assignedRoles = user.UserRoles.Select(userRole => userRole.Role.Name).ToArray();

        return new ProvisionedIdentityDto(user.Id, user.Email, assignedRoles);
    }
}