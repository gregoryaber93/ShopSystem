using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Abstractions.Persistence;
using OrderService.Contracts.Dtos;

namespace OrderService.Application.Features.Orders.Queries.ReplayOrderProjection;

public sealed class ReplayOrderProjectionQueryHandler(
    IOrderProjectionRebuilder orderProjectionRebuilder) : IQueryHandler<ReplayOrderProjectionQuery, OrderDto?>
{
    public Task<OrderDto?> Handle(ReplayOrderProjectionQuery query, CancellationToken cancellationToken)
    {
        if (query.OrderId == Guid.Empty)
        {
            throw new ArgumentException("OrderId is required.");
        }

        return orderProjectionRebuilder.RebuildAsync(query.OrderId, cancellationToken);
    }
}
