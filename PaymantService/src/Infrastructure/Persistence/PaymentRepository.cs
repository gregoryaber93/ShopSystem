using Microsoft.EntityFrameworkCore;
using PaymantService.Application.Abstractions.Persistence;
using PaymantService.Domain.Entities;

namespace PaymantService.Infrastructure.Persistence;

public sealed class PaymentRepository(PaymentDbContext dbContext) : IPaymentRepository
{
    public Task AddAsync(PaymentEntity payment, CancellationToken cancellationToken)
    {
        return dbContext.Payments.AddAsync(payment, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<PaymentEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.UserId == userId)
            .OrderByDescending(payment => payment.PaidAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PaymentEntity>> GetByShopIdAsync(Guid shopId, CancellationToken cancellationToken)
    {
        return await dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.ShopId == shopId)
            .OrderByDescending(payment => payment.PaidAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}


