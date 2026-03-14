using AuthenticationService.Application.Abstractions.CQRS;
using AuthenticationService.Contracts.Dtos;

namespace AuthenticationService.Application.Features.Authentication.Commands.ProvisionIdentity;

public sealed record ProvisionIdentityCommand(ProvisionIdentityRequestDto Request) : ICommand<ProvisionedIdentityDto?>;