using OrderService.Domain.Entities;

namespace OrderService.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task AddAsync(OrderEntity order, CancellationToken cancellationToken);
    Task UpsertAsync(OrderEntity order, CancellationToken cancellationToken);
    Task<OrderEntity?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OrderEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OrderEntity>> GetByShopIdAsync(Guid shopId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
