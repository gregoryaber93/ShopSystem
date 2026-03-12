using OrderService.Application.Abstractions.CQRS;
using OrderService.Contracts.Dtos;

namespace OrderService.Application.Features.Orders.Queries.ReplayOrderProjection;

public sealed record ReplayOrderProjectionQuery(Guid OrderId) : IQuery<OrderDto?>;
