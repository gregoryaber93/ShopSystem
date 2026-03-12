using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace OrderService.Infrastructure.Security;

internal sealed class RsaJwtTokenService(IOptions<JwtRsaOptions> options) : IJwtTokenService
{
    private readonly JwtRsaOptions _options = options.Value;

    public string CreateServiceToken(string subject)
    {
        if (string.IsNullOrWhiteSpace(_options.PrivateKeyXml))
        {
            throw new InvalidOperationException("JwtRsa:PrivateKeyXml is required for service-to-service JWT signing.");
        }

        using var rsa = RSA.Create();
        rsa.FromXmlString(_options.PrivateKeyXml);

        var credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
        var now = DateTime.UtcNow;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_options.TokenLifetimeMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
