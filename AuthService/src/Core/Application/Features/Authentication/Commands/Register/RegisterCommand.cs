using AuthService.Application.Abstractions.CQRS;
using AuthService.Contracts.Dtos;

namespace AuthService.Application.Features.Authentication.Commands.Register;

public sealed record RegisterCommand(RegisterRequestDto Request) : ICommand<AuthResponseDto?>;
