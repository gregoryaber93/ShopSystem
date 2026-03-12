namespace AuthenticationService.Contracts.Dtos;

public sealed record RegisterRequestDto(string Email, string Password, IReadOnlyCollection<string>? Roles = null);
