using ShopService.Domain.Entities;

namespace ShopService.Application.Abstractions.Persistence;

public interface IShopRepository
{
    Task<IReadOnlyCollection<ShopEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<ShopEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ShopEntity shop, CancellationToken cancellationToken);
    void Remove(ShopEntity shop);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}