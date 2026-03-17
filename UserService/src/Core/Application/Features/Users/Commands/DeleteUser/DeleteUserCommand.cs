using UserService.Application.Abstractions.CQRS;

namespace UserService.Application.Features.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : ICommand<bool>;