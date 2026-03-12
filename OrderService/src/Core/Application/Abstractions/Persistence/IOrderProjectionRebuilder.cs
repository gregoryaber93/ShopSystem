using OrderService.Contracts.Dtos;

namespace OrderService.Application.Abstractions.Persistence;

public interface IOrderProjectionRebuilder
{
    Task<OrderDto?> RebuildAsync(Guid orderId, CancellationToken cancellationToken);
}
