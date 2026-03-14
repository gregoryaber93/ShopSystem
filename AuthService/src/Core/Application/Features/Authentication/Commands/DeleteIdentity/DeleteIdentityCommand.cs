using AuthService.Application.Abstractions.CQRS;

namespace AuthService.Application.Features.Authentication.Commands.DeleteIdentity;

public sealed record DeleteIdentityCommand(Guid UserId) : ICommand<bool>;