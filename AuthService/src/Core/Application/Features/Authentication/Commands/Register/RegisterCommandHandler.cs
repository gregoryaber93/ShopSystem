using AuthService.Application.Abstractions.CQRS;
using AuthService.Application.Abstractions.Outbox;
using AuthService.Application.Abstractions.Persistence;
using AuthService.Application.Abstractions.Profiles;
using AuthService.Application.Abstractions.Security;
using AuthService.Application.Common;
using AuthService.Contracts.Dtos;
using AuthService.Domain.Entities;

namespace AuthService.Application.Features.Authentication.Commands.Register;

public sealed class RegisterCommandHandler(
    IAuthOutboxService authOutboxService,
    IAuthUserRepository authUserRepository,
    IPasswordHasherService passwordHasherService,
    IUserProfileProvisioningClient userProfileProvisioningClient,
    IJwtTokenService jwtTokenService) : ICommandHandler<RegisterCommand, AuthResponseDto?>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "User"
    };

    public async Task<AuthResponseDto?> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim().ToLowerInvariant();
        if (await authUserRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return null;
        }


        var roles = new List<string> { "User" };

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
        var outboxMessageId = await authOutboxService.EnqueueUserCreatedAsync(user.Id, user.Email, roles, cancellationToken);
        await authUserRepository.SaveChangesAsync(cancellationToken);

        var assignedRoles = user.UserRoles.Select(userRole => userRole.Role.Name).ToArray();

        try
        {
            var profileCreated = await userProfileProvisioningClient.CreateOrUpdateProfileAsync(
                user.Id,
                user.Email,
                assignedRoles,
                cancellationToken);

            if (!profileCreated)
            {
                await authOutboxService.MarkPermanentFailureAsync(outboxMessageId, "Profile conflict in UserService.", cancellationToken);
                await authUserRepository.SaveChangesAsync(cancellationToken);
                throw new ProfileProvisioningException("Nie mozna utworzyc profilu w UserService, poniewaz email jest juz powiazany z innym profilem.", true);
            }

            await authOutboxService.MarkCompletedAsync(outboxMessageId, cancellationToken);
            await authUserRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ProfileProvisioningException)
        {
            throw;
        }
        catch (Exception exception)
        {
            var retryAfter = DateTime.UtcNow.AddSeconds(30);
            await authOutboxService.MarkRetryableFailureAsync(outboxMessageId, exception.Message, retryAfter, cancellationToken);
            await authUserRepository.SaveChangesAsync(cancellationToken);
            throw new ProfileProvisioningException("Nie udalo sie zsynchronizowac profilu z UserService. Rejestracja zostanie dokonczona przez mechanizm rekonsyliacji.", false, exception);
        }

        var (token, expiresAtUtc) = jwtTokenService.CreateUserToken(user.Id, user.Email, assignedRoles);

        return new AuthResponseDto(token, expiresAtUtc, user.Id, user.Email, assignedRoles);
    }
}
