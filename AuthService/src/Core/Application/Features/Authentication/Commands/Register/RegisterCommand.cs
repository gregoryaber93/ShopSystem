using AuthenticationService.Application.Abstractions.CQRS;
using AuthenticationService.Contracts.Dtos;

namespace AuthenticationService.Application.Features.Authentication.Commands.Register;

public sealed record RegisterCommand(RegisterRequestDto Request) : ICommand<AuthResponseDto?>;
