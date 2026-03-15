using OrderService.Application.Abstractions.CQRS;
using OrderService.Contracts.Dtos;

namespace OrderService.Application.Features.Orders.Commands.PlaceOrderFromCart;

public sealed record PlaceOrderFromCartCommand(Guid UserId, PlaceOrderFromCartRequestDto Request) : ICommand<CartCheckoutResultDto>;
