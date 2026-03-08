using Microsoft.EntityFrameworkCore;
using ShopService.Application.Abstractions.Persistence;
using ShopService.Domain.Entities;

namespace ShopService.Infrastructure.Persistence;

public sealed class ShopRepository(ShopDbContext dbContext) : IShopRepository
{
    public async Task<IReadOnlyCollection<ShopEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Shops
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<ShopEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Shops.FirstOrDefaultAsync(shop => shop.Id == id, cancellationToken);
    }

    public Task AddAsync(ShopEntity shop, CancellationToken cancellationToken)
    {
        return dbContext.Shops.AddAsync(shop, cancellationToken).AsTask();
    }

    public void Remove(ShopEntity shop)
    {
        dbContext.Shops.Remove(shop);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}