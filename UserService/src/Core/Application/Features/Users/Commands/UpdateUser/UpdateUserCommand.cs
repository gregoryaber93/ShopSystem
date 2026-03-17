using UserService.Application.Abstractions.CQRS;
using UserService.Contracts.Dtos;

namespace UserService.Application.Features.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(Guid UserId, UpdateUserRequestDto Request) : ICommand<UserDto?>;