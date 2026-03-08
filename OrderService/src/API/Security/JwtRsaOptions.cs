namespace OrderService.Api.Security;

public sealed class JwtRsaOptions
{
    public const string SectionName = "JwtRsa";

    public string Issuer { get; set; } = "ShopSystem.Auth";

    public string Audience { get; set; } = "ShopSystem.Services";

    public string PublicKeyXml { get; set; } = string.Empty;
}
