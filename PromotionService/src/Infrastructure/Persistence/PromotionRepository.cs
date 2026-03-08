using Microsoft.EntityFrameworkCore;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Domain.Entities;

namespace PromotionService.Infrastructure.Persistence;

public sealed class PromotionRepository(PromotionDbContext dbContext) : IPromotionRepository
{
    public async Task<IReadOnlyCollection<PromotionEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Promotions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<PromotionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Promotions.FirstOrDefaultAsync(promotion => promotion.Id == id, cancellationToken);
    }

    public Task AddAsync(PromotionEntity promotion, CancellationToken cancellationToken)
    {
        return dbContext.Promotions.AddAsync(promotion, cancellationToken).AsTask();
    }

    public void Remove(PromotionEntity promotion)
    {
        dbContext.Promotions.Remove(promotion);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
