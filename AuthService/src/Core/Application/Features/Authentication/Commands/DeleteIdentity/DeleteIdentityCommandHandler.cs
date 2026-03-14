using AuthenticationService.Application.Abstractions.CQRS;
using AuthenticationService.Application.Abstractions.Persistence;

namespace AuthenticationService.Application.Features.Authentication.Commands.DeleteIdentity;

public sealed class DeleteIdentityCommandHandler(
    IAuthUserRepository authUserRepository) : ICommandHandler<DeleteIdentityCommand, bool>
{
    public async Task<bool> Handle(DeleteIdentityCommand command, CancellationToken cancellationToken)
    {
        var user = await authUserRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        authUserRepository.RemoveUser(user);
        await authUserRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}