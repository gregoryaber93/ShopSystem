using UserService.Application.Abstractions.CQRS;
using UserService.Application.Abstractions.Persistence;
using UserService.Application.Abstractions.Security;
using UserService.Contracts.Dtos;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService,
    ICurrentUserService currentUserService) : ICommandHandler<CreateUserCommand, UserDto?>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Manager",
        "User"
    };

    public async Task<UserDto?> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsInRole("Admin"))
        {
            throw new UnauthorizedAccessException("Tylko administrator moze tworzyc nowych uzytkownikow.");
        }

        var email = command.Request.Email.Trim().ToLowerInvariant();
        if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
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

        var roleEntities = await userRepository.GetOrCreateRolesAsync(roles, cancellationToken);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasherService.HashPassword(command.Request.Password)
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

        var assignedRoles = user.UserRoles.Select(userRole => userRole.Role.Name).ToArray();
        return new UserDto(user.Id, user.Email, assignedRoles);
    }
}
