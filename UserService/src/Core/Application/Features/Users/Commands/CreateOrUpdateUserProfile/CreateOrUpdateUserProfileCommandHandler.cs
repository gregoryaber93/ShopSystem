using UserService.Application.Abstractions.CQRS;
using UserService.Application.Abstractions.Persistence;
using UserService.Contracts.Dtos;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Users.Commands.CreateOrUpdateUserProfile;

public sealed class CreateOrUpdateUserProfileCommandHandler(
    IUserRepository userRepository) : ICommandHandler<CreateOrUpdateUserProfileCommand, UserDto?>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Manager",
        "User"
    };

    public async Task<UserDto?> Handle(CreateOrUpdateUserProfileCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim().ToLowerInvariant();
        var roles = (command.Request.Roles ?? [])
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

        var existingById = await userRepository.GetByIdWithRolesAsync(command.Request.UserId, cancellationToken);
        var existingByEmail = await userRepository.GetByEmailWithRolesAsync(email, cancellationToken);

        if (existingByEmail is not null && existingByEmail.Id != command.Request.UserId)
        {
            return null;
        }

        var roleEntities = await userRepository.GetOrCreateRolesAsync(roles, cancellationToken);

        if (existingById is null)
        {
            var user = new UserEntity
            {
                Id = command.Request.UserId,
                Email = email
            };

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

            await userRepository.AddUserAsync(user, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);

            var createdRoles = user.UserRoles.Select(userRole => userRole.Role.Name).ToArray();
            return new UserDto(user.Id, user.Email, createdRoles);
        }

        existingById.Email = email;

        var requestedRoles = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingRoleNames = existingById.UserRoles
            .Select(userRole => userRole.Role.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roleEntities)
        {
            if (existingRoleNames.Contains(role.Name))
            {
                continue;
            }

            existingById.UserRoles.Add(new UserRoleEntity
            {
                UserId = existingById.Id,
                RoleId = role.Id,
                User = existingById,
                Role = role
            });
        }

        var rolesToRemove = existingById.UserRoles
            .Where(userRole => !requestedRoles.Contains(userRole.Role.Name))
            .ToArray();

        foreach (var userRole in rolesToRemove)
        {
            userRepository.RemoveUserRole(userRole);
        }

        await userRepository.SaveChangesAsync(cancellationToken);

        var assignedRoles = existingById.UserRoles
            .Where(userRole => requestedRoles.Contains(userRole.Role.Name))
            .Select(userRole => userRole.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new UserDto(existingById.Id, existingById.Email, assignedRoles);
    }
}
