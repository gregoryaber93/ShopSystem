using AuthService.Application.Abstractions.CQRS;
using AuthService.Application.Features.Authentication.Commands.DeleteIdentity;
using AuthService.Application.Features.Authentication.Commands.ProvisionIdentity;
using AuthService.Application.Features.Authentication.Commands.UpdateIdentity;
using AuthService.Contracts.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth/internal")]
[Authorize(Roles = "Admin")]
public sealed class InternalAuthenticationController(
    ICommandHandler<ProvisionIdentityCommand, ProvisionedIdentityDto?> provisionIdentityCommandHandler,
    ICommandHandler<UpdateIdentityCommand, ProvisionedIdentityDto?> updateIdentityCommandHandler,
    ICommandHandler<DeleteIdentityCommand, bool> deleteIdentityCommandHandler) : ControllerBase
{
    [HttpPost("provision-user")]
    public async Task<ActionResult<ProvisionedIdentityDto>> ProvisionUser([FromBody] ProvisionIdentityRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await provisionIdentityCommandHandler.Handle(new ProvisionIdentityCommand(request), cancellationToken);
            if (result is null)
            {
                return Conflict("Uzytkownik o podanym emailu juz istnieje.");
            }

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("users/{userId:guid}")]
    public async Task<ActionResult<ProvisionedIdentityDto>> UpdateProvisionedUser(
        Guid userId,
        [FromBody] UpdateIdentityRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await updateIdentityCommandHandler.Handle(new UpdateIdentityCommand(userId, request), cancellationToken);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> DeleteProvisionedUser(Guid userId, CancellationToken cancellationToken)
    {
        var deleted = await deleteIdentityCommandHandler.Handle(new DeleteIdentityCommand(userId), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}