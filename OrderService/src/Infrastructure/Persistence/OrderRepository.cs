using Microsoft.EntityFrameworkCore;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public sealed class OrderRepository(OrderDbContext dbContext) : IOrderRepository
{
    public Task AddAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        return dbContext.Orders.AddAsync(order, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<OrderEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.OrderedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrderEntity>> GetByShopIdAsync(Guid shopId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(order => order.ShopId == shopId)
            .OrderByDescending(order => order.OrderedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
