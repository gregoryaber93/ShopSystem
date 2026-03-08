using UserService.Application.Abstractions.CQRS;
using UserService.Contracts.Dtos;

namespace UserService.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(CreateUserRequestDto Request) : ICommand<UserDto?>;
