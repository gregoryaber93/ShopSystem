using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserService.Application.Abstractions.CQRS;
using UserService.Application.Features.Users.Commands.CreateOrUpdateUserProfile;
using UserService.Contracts.Dtos;
using UserService.Infrastructure.Security;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/users/internal")]
[AllowAnonymous]
public sealed class InternalUsersController(
    ICommandHandler<CreateOrUpdateUserProfileCommand, UserDto?> createOrUpdateUserProfileCommandHandler,
    IOptions<InternalApiOptions> internalApiOptions) : ControllerBase
{
    [HttpPost("create-or-update-profile")]
    public async Task<ActionResult<UserDto>> CreateOrUpdateProfile([FromBody] CreateOrUpdateUserProfileRequestDto request, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized("Brak poprawnego klucza wewnetrznego.");
        }

        try
        {
            var result = await createOrUpdateUserProfileCommandHandler.Handle(new CreateOrUpdateUserProfileCommand(request), cancellationToken);
            if (result is null)
            {
                return Conflict("Profil o podanym emailu jest juz powiazany z innym uzytkownikiem.");
            }

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    private bool IsAuthorizedInternalRequest()
    {
        var configuredKey = internalApiOptions.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            return false;
        }

        if (!Request.Headers.TryGetValue("X-Internal-Api-Key", out var providedHeader))
        {
            return false;
        }

        var providedKey = providedHeader.ToString();
        return string.Equals(providedKey, configuredKey, StringComparison.Ordinal);
    }
}
