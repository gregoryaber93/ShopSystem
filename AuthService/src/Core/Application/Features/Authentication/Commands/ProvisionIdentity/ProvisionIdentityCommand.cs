using AuthService.Application.Abstractions.CQRS;
using AuthService.Contracts.Dtos;

namespace AuthService.Application.Features.Authentication.Commands.ProvisionIdentity;

public sealed record ProvisionIdentityCommand(ProvisionIdentityRequestDto Request) : ICommand<ProvisionedIdentityDto?>;