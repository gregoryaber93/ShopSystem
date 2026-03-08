using PaymantService.Domain.Entities;

namespace PaymantService.Application.Abstractions.Persistence;

public interface IPaymentRepository
{
    Task AddAsync(PaymentEntity payment, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PaymentEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PaymentEntity>> GetByShopIdAsync(Guid shopId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}


