using AuthService.Application.Abstractions.CQRS;
using AuthService.Contracts.Dtos;

namespace AuthService.Application.Features.Authentication.Commands.Login;

public sealed record LoginCommand(LoginRequestDto Request) : ICommand<AuthResponseDto?>;
