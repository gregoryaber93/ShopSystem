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

    public async Task UpsertAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Orders
            .FirstOrDefaultAsync(item => item.Id == order.Id, cancellationToken);

        if (existing is null)
        {
            await dbContext.Orders.AddAsync(order, cancellationToken);
            return;
        }

        existing.UserId = order.UserId;
        existing.ShopId = order.ShopId;
        existing.ProductId = order.ProductId;
        existing.Quantity = order.Quantity;
        existing.UnitPrice = order.UnitPrice;
        existing.TotalPrice = order.TotalPrice;
        existing.OrderedAtUtc = order.OrderedAtUtc;
        existing.Status = order.Status;
    }

    public Task<OrderEntity?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
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
