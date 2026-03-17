using AuthService.Application.Abstractions.CQRS;
using AuthService.Contracts.Dtos;

namespace AuthService.Application.Features.Authentication.Commands.UpdateIdentity;

public sealed record UpdateIdentityCommand(Guid UserId, UpdateIdentityRequestDto Request) : ICommand<ProvisionedIdentityDto?>;