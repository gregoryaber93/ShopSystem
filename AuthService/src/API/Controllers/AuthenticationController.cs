using AuthenticationService.Application.Abstractions.CQRS;
using AuthenticationService.Application.Features.Authentication.Commands.Login;
using AuthenticationService.Application.Features.Authentication.Commands.Register;
using AuthenticationService.Contracts.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
