namespace UserService.Infrastructure.Security;

public sealed class JwtRsaOptions
{
    public const string SectionName = "JwtRsa";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int TokenLifetimeMinutes { get; init; } = 60;
    public string PublicKeyXml { get; init; } = string.Empty;
}
