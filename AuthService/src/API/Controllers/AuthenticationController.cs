using AuthService.Application.Abstractions.CQRS;
using AuthService.Application.Common;
using AuthService.Application.Features.Authentication.Commands.Login;
using AuthService.Application.Features.Authentication.Commands.Register;
using AuthService.Contracts.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthenticationController(
    ICommandHandler<RegisterCommand, AuthResponseDto?> registerCommandHandler,
    ICommandHandler<LoginCommand, AuthResponseDto?> loginCommandHandler) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await registerCommandHandler.Handle(new RegisterCommand(request), cancellationToken);
            if (result is null)
            {
                return Conflict("Uzytkownik o podanym emailu juz istnieje.");
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ProfileProvisioningException exception) when (exception.IsConflict)
        {
            return Conflict(exception.Message);
        }
        catch (ProfileProvisioningException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, exception.Message);
        }
    }

    [HttpPost("registerTest")]
    public async Task<ActionResult<AuthResponseDto>> RegisterTest([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        return Ok("registerTest");
    }


    [HttpPost("loginTest")]
    public async Task<ActionResult<AuthResponseDto>> LoginTest([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        return Ok("loginTest");
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await loginCommandHandler.Handle(new LoginCommand(request), cancellationToken);
        if (result is null)
        {
            return Unauthorized();
        }

        return Ok(result);
    }
}
