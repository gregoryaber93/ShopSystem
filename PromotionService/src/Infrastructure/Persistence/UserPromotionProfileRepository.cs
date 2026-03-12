using Microsoft.EntityFrameworkCore;
using PromotionService.Application.Abstractions.Persistence;
using PromotionService.Domain.Entities;

namespace PromotionService.Infrastructure.Persistence;

public sealed class UserPromotionProfileRepository(PromotionDbContext dbContext) : IUserPromotionProfileRepository
{
    public Task<UserPromotionProfileEntity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return dbContext.UserPromotionProfiles.FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public async Task UpsertAsync(UserPromotionProfileEntity profile, CancellationToken cancellationToken)
    {
        var existing = await dbContext.UserPromotionProfiles
            .FirstOrDefaultAsync(p => p.UserId == profile.UserId, cancellationToken);

        if (existing is null)
        {
            await dbContext.UserPromotionProfiles.AddAsync(profile, cancellationToken);
            return;
        }

        existing.LoyaltyPoints = profile.LoyaltyPoints;
        existing.OrdersCount = profile.OrdersCount;
        existing.TotalSpent = profile.TotalSpent;
        existing.LastOrderAtUtc = profile.LastOrderAtUtc;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
