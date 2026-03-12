using OrderService.Application.Abstractions.CQRS;
using OrderService.Contracts.Dtos;

namespace OrderService.Application.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto?>;
