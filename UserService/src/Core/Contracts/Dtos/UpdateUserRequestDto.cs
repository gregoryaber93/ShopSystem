namespace UserService.Contracts.Dtos;

public sealed record UpdateUserRequestDto(string Email, IReadOnlyCollection<string> Roles, string? Password);