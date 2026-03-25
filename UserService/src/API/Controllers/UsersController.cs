using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Abstractions.CQRS;
using UserService.Application.Features.Users.Commands.CreateUser;
using UserService.Application.Features.Users.Commands.DeleteUser;
using UserService.Application.Features.Users.Commands.UpdateUser;
using UserService.Application.Features.Users.Queries.GetUsers;
using UserService.Contracts.Dtos;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(
    IQueryHandler<GetUsersQuery, IReadOnlyCollection<UserDto>> getUsersQueryHandler,
    ICommandHandler<CreateUserCommand, UserDto?> createUserCommandHandler,
    ICommandHandler<UpdateUserCommand, UserDto?> updateUserCommandHandler,
    ICommandHandler<DeleteUserCommand, bool> deleteUserCommandHandler) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> GetUsers([FromQuery] string? role, CancellationToken cancellationToken)
    {
        var response = await getUsersQueryHandler.Handle(new GetUsersQuery(role), cancellationToken);

        return Ok(response);
    }

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

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid userId, [FromBody] UpdateUserRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await updateUserCommandHandler.Handle(new UpdateUserCommand(userId, request), cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(updated);
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("{userId:guid}")]
    public async Task<ActionResult> DeleteUser(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await deleteUserCommandHandler.Handle(new DeleteUserCommand(userId), cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
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
