namespace ProductService.Domain.Entities;

public sealed class ProductEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid ShopId { get; set; }
}
