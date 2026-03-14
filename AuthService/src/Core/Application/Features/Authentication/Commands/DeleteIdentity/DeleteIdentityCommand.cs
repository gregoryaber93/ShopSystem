using AuthenticationService.Application.Abstractions.CQRS;

namespace AuthenticationService.Application.Features.Authentication.Commands.DeleteIdentity;

public sealed record DeleteIdentityCommand(Guid UserId) : ICommand<bool>;