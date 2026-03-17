using UserService.Application.Abstractions.CQRS;
using UserService.Application.Abstractions.Identity;
using UserService.Application.Abstractions.Persistence;
using UserService.Application.Abstractions.Security;

namespace UserService.Application.Features.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IAuthIdentityProvisioningClient authIdentityProvisioningClient,
    ICurrentUserService currentUserService) : ICommandHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsInRole("Admin"))
        {
            throw new UnauthorizedAccessException("Tylko administrator moze usuwac uzytkownikow.");
        }

        if (command.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.");
        }

        var user = await userRepository.GetByIdWithRolesAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        if (!user.UserRoles.Any(userRole => string.Equals(userRole.Role.Name, "Manager", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Mozna usunac tylko konto managera.");
        }

        await authIdentityProvisioningClient.RollbackProvisionAsync(user.Id, cancellationToken);

        userRepository.RemoveUser(user);
        await userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}