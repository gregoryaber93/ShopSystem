namespace AuthService.Application.Abstractions.Security;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateUserToken(Guid userId, string email, IReadOnlyCollection<string> roles);
}
