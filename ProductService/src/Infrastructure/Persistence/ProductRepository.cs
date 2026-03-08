using Microsoft.EntityFrameworkCore;
using ProductService.Application.Abstractions.Persistence;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Persistence;

public sealed class ProductRepository(ProductDbContext dbContext) : IProductRepository
{
    public async Task<IReadOnlyCollection<ProductEntity>> GetAllByShopIdAsync(Guid shopId, CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(product => product.ShopId == shopId)
            .ToListAsync(cancellationToken);
    }

    public Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public Task AddAsync(ProductEntity product, CancellationToken cancellationToken)
    {
        return dbContext.Products.AddAsync(product, cancellationToken).AsTask();
    }

    public void Remove(ProductEntity product)
    {
        dbContext.Products.Remove(product);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
