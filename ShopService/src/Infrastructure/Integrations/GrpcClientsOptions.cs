namespace ShopService.Infrastructure.Integrations;

public sealed class GrpcClientsOptions
{
    public const string SectionName = "GrpcClients";

    public string Products { get; set; } = "https://localhost:7166";

    public string Promotions { get; set; } = "https://localhost:7165";

    public string Orders { get; set; } = "https://localhost:7170";
}