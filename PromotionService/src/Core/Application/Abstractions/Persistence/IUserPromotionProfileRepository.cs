using PromotionService.Domain.Entities;

namespace PromotionService.Application.Abstractions.Persistence;

public interface IUserPromotionProfileRepository
{
    Task<UserPromotionProfileEntity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task UpsertAsync(UserPromotionProfileEntity profile, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
