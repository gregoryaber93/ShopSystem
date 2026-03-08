using OrderService.Application.Abstractions.CQRS;
using OrderService.Contracts.Dtos;

namespace OrderService.Application.Features.Orders.Queries.GetOrdersByShop;

public sealed record GetOrdersByShopQuery(Guid ShopId) : IQuery<IReadOnlyCollection<OrderDto>>;
