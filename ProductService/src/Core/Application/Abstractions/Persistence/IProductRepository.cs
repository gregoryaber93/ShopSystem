using ProductService.Domain.Entities;

namespace ProductService.Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task<IReadOnlyCollection<ProductEntity>> GetAllByShopIdAsync(Guid shopId, CancellationToken cancellationToken);
    Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ProductEntity product, CancellationToken cancellationToken);
    void Remove(ProductEntity product);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
