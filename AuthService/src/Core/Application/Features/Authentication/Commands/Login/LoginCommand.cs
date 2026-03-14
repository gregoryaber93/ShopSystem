using AuthenticationService.Application.Abstractions.CQRS;
using AuthenticationService.Contracts.Dtos;

namespace AuthenticationService.Application.Features.Authentication.Commands.Login;

public sealed record LoginCommand(LoginRequestDto Request) : ICommand<AuthResponseDto?>;
