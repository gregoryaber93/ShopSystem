namespace AuthService.Contracts.Dtos;

public sealed record UpdateIdentityRequestDto(string Email, IReadOnlyCollection<string> Roles, string? Password);