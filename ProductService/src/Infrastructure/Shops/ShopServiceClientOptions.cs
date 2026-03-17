namespace ProductService.Infrastructure.Shops;

public sealed class ShopServiceClientOptions
{
    public const string SectionName = "ShopService";
    public string BaseUrl { get; init; } = string.Empty;
}
