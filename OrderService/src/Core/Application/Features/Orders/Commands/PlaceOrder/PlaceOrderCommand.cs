using OrderService.Application.Abstractions.CQRS;
using OrderService.Contracts.Dtos;

namespace OrderService.Application.Features.Orders.Commands.PlaceOrder;

public sealed record PlaceOrderCommand(Guid UserId, PlaceOrderRequestDto Request) : ICommand<OrderDto>;
