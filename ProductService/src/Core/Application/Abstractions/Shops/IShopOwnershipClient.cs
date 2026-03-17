namespace ProductService.Application.Abstractions.Shops;

public interface IShopOwnershipClient
{
    Task<Guid?> GetShopOwnerAsync(Guid shopId, CancellationToken cancellationToken);
}
