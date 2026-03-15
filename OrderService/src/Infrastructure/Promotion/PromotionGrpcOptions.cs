namespace OrderService.Infrastructure.Promotion;

public sealed class PromotionGrpcOptions
{
    public const string SectionName = "Grpc:PromotionService";

    public string Address { get; set; } = "https://localhost:7165";
}
