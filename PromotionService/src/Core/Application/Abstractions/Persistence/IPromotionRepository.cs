using PromotionService.Domain.Entities;

namespace PromotionService.Application.Abstractions.Persistence;

public interface IPromotionRepository
{
    Task<IReadOnlyCollection<PromotionEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<PromotionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(PromotionEntity promotion, CancellationToken cancellationToken);
    void Remove(PromotionEntity promotion);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
