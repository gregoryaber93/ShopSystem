namespace ShopService.Infrastructure.Security;

public sealed class JwtRsaOptions
{
    public const string SectionName = "JwtRsa";

    public string Issuer { get; set; } = "ShopSystem.Auth";

    public string Audience { get; set; } = "ShopSystem.Services";

    public int TokenLifetimeMinutes { get; set; } = 30;

    public string PublicKeyXml { get; set; } = string.Empty;

    public string PrivateKeyXml { get; set; } = string.Empty;
}
