namespace OrderService.Application.Abstractions.Integrations;

public interface IProductCatalogGateway
{
    Task<ProductSnapshot?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken);
}
