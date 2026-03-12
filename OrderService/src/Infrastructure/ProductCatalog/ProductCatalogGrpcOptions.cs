namespace OrderService.Infrastructure.ProductCatalog;

public sealed class ProductCatalogGrpcOptions
{
    public const string SectionName = "Grpc:ProductService";

    public string Address { get; set; } = "https://localhost:7166";
}
