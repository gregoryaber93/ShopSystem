using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Features.Orders.Commands.PlaceOrder;
using OrderService.Application.Features.Orders.Queries.GetMyOrders;
using OrderService.Application.Features.Orders.Queries.GetOrdersByShop;
using OrderService.Contracts.Dtos;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,User")]
public class OrdersController(
    ICommandHandler<PlaceOrderCommand, OrderDto> placeOrderCommandHandler,
    IQueryHandler<GetMyOrdersQuery, IReadOnlyCollection<OrderDto>> getMyOrdersQueryHandler,
    IQueryHandler<GetOrdersByShopQuery, IReadOnlyCollection<OrderDto>> getOrdersByShopQueryHandler) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] PlaceOrderRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = ResolveUserId(User);
            var order = await placeOrderCommandHandler.Handle(new PlaceOrderCommand(userId, request), cancellationToken);
            return Created($"/api/orders/{order.Id}", order);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                title: "gRPC integration failure",
                detail: exception.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    [HttpGet("my")]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<IReadOnlyCollection<OrderDto>>> GetMyOrders(CancellationToken cancellationToken)
    {
        try
        {
            var userId = ResolveUserId(User);
            var orders = await getMyOrdersQueryHandler.Handle(new GetMyOrdersQuery(userId), cancellationToken);
            return Ok(orders);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("shop/{shopId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IReadOnlyCollection<OrderDto>>> GetOrdersByShop(Guid shopId, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await getOrdersByShopQueryHandler.Handle(new GetOrdersByShopQuery(shopId), cancellationToken);
            return Ok(orders);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    private static Guid ResolveUserId(ClaimsPrincipal principal)
    {
        var subValue = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(subValue, out var userId) || userId == Guid.Empty)
        {
            throw new ArgumentException("Cannot resolve user id from token.");
        }

        return userId;
    }
}
