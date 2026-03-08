using OrderService.Application.Abstractions.CQRS;
using OrderService.Contracts.Dtos;

namespace OrderService.Application.Features.Orders.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery(Guid UserId) : IQuery<IReadOnlyCollection<OrderDto>>;
