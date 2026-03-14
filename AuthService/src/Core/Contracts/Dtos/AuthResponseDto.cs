namespace AuthService.Contracts.Dtos;

public sealed record AuthResponseDto(string AccessToken, DateTime ExpiresAtUtc, Guid UserId, string Email, IReadOnlyCollection<string> Roles);
