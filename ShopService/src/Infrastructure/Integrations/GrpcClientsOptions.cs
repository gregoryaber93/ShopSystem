namespace ShopService.Infrastructure.Integrations;

public sealed class GrpcClientsOptions
{
    public const string SectionName = "GrpcClients";

    public string Products { get; set; } = "https://localhost:7001";

    public string Promotions { get; set; } = "https://localhost:7002";

    public string Orders { get; set; } = "https://localhost:7003";
}