using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Abstractions.CQRS;
using UserService.Application.Features.Users.Commands.CreateUser;
using UserService.Contracts.Dtos;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController(
    ICommandHandler<CreateUserCommand, UserDto?> createUserCommandHandler) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await createUserCommandHandler.Handle(new CreateUserCommand(request), cancellationToken);
            if (created is null)
            {
                return Conflict("Uzytkownik o podanym emailu juz istnieje.");
            }

            return Ok(created);
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
