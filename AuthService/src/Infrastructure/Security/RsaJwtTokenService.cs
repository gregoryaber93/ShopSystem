using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AuthService.Application.Abstractions.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Security;

internal sealed class RsaJwtTokenService(IOptions<JwtRsaOptions> options) : IJwtTokenService
{
    private readonly JwtRsaOptions _options = options.Value;

    public (string Token, DateTime ExpiresAtUtc) CreateUserToken(Guid userId, string email, IReadOnlyCollection<string> roles)
    {
        if (string.IsNullOrWhiteSpace(_options.PrivateKeyXml))
        {
            throw new InvalidOperationException("JwtRsa:PrivateKeyXml is required for JWT signing.");
        }

        var rsa = RSA.Create();
        rsa.FromXmlString(_options.PrivateKeyXml);

        var credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_options.TokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (jwt, expires);
    }
}
