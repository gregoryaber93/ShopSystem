using UserService.Application.Abstractions.CQRS;
using UserService.Application.Abstractions.Identity;
using UserService.Application.Abstractions.Persistence;
using UserService.Application.Abstractions.Security;
using UserService.Contracts.Dtos;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IAuthIdentityProvisioningClient authIdentityProvisioningClient,
    ICurrentUserService currentUserService) : ICommandHandler<UpdateUserCommand, UserDto?>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Manager",
        "User"
    };

    public async Task<UserDto?> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsInRole("Admin"))
        {
            throw new UnauthorizedAccessException("Tylko administrator moze aktualizowac uzytkownikow.");
        }

        if (command.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.");
        }

        var user = await userRepository.GetByIdWithRolesAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (!user.UserRoles.Any(userRole => string.Equals(userRole.Role.Name, "Manager", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Mozna edytowac tylko konto managera.");
        }

        var email = command.Request.Email.Trim().ToLowerInvariant();
        var existingByEmail = await userRepository.GetByEmailWithRolesAsync(email, cancellationToken);
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
            roles = ["Manager"];
        }

        if (roles.Any(role => string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Rola Admin nie moze byc ustawiona przez endpoint aktualizacji uzytkownika.");
        }

        var invalidRoles = roles.Where(role => !AllowedRoles.Contains(role)).ToArray();
        if (invalidRoles.Length > 0)
        {
            throw new ArgumentException($"Nieobslugiwane role: {string.Join(", ", invalidRoles)}");
        }

        if (!roles.Any(role => string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Aktualizowany uzytkownik musi zachowac role Manager.");
        }

        var updatedIdentity = await authIdentityProvisioningClient.UpdateUserAsync(
            user.Id,
            email,
            roles,
            command.Request.Password,
            cancellationToken);

        if (updatedIdentity is null)
        {
            return null;
        }

        user.Email = email;

        var existingRoles = user.UserRoles.ToArray();
        foreach (var userRole in existingRoles)
        {
            userRepository.RemoveUserRole(userRole);
        }

        var roleEntities = await userRepository.GetOrCreateRolesAsync(roles, cancellationToken);
        foreach (var role in roleEntities)
        {
            user.UserRoles.Add(new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = role.Id,
                User = user,
                Role = role
            });
        }

        await userRepository.SaveChangesAsync(cancellationToken);
        var assignedRoles = user.UserRoles.Select(userRole => userRole.Role.Name).ToArray();

        return new UserDto(user.Id, user.Email, assignedRoles);
    }
}