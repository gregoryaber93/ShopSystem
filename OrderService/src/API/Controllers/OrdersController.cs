using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Abstractions.CQRS;
using OrderService.Application.Features.Orders.Commands.PlaceOrder;
using OrderService.Application.Features.Orders.Commands.PlaceOrderFromCart;
using OrderService.Application.Features.Orders.Queries.GetMyOrders;
using OrderService.Application.Features.Orders.Queries.GetOrdersByShop;
using OrderService.Application.Features.Orders.Queries.ReplayOrderProjection;
using OrderService.Contracts.Dtos;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = "Admin,Manager,User")]
public class OrdersController(
    ICommandHandler<PlaceOrderCommand, OrderDto> placeOrderCommandHandler,
    ICommandHandler<PlaceOrderFromCartCommand, CartCheckoutResultDto> placeOrderFromCartCommandHandler,
    IDistributedCache distributedCache,
    IQueryHandler<GetMyOrdersQuery, IReadOnlyCollection<OrderDto>> getMyOrdersQueryHandler,
    IQueryHandler<GetOrdersByShopQuery, IReadOnlyCollection<OrderDto>> getOrdersByShopQueryHandler,
    IQueryHandler<ReplayOrderProjectionQuery, OrderDto?> replayOrderProjectionQueryHandler) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] PlaceOrderRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var idempotencyKey = Request.Headers["Idempotency-Key"].ToString();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return BadRequest(new { error = "Idempotency-Key header is required." });
            }

            var userId = ResolveUserId(User);
            var cacheKey = $"idempotency:orders:{userId:N}:{idempotencyKey}";
            var existingOrderId = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(existingOrderId) && Guid.TryParse(existingOrderId, out var parsedOrderId))
            {
                return Ok(new { orderId = parsedOrderId, duplicate = true });
            }

            var order = await placeOrderCommandHandler.Handle(new PlaceOrderCommand(userId, request), cancellationToken);
            await distributedCache.SetStringAsync(cacheKey, order.Id.ToString("D"), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
            }, cancellationToken);

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

    [HttpPost("cart-checkout")]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<CartCheckoutResultDto>> PlaceOrderFromCart([FromBody] PlaceOrderFromCartRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var idempotencyKey = Request.Headers["Idempotency-Key"].ToString();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return BadRequest(new { error = "Idempotency-Key header is required." });
            }

            var userId = ResolveUserId(User);
            var cacheKey = $"idempotency:orders-cart:{userId:N}:{idempotencyKey}";
            var cached = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                return Ok(new { duplicate = true, message = "Cart checkout already processed for this Idempotency-Key." });
            }

            var checkout = await placeOrderFromCartCommandHandler.Handle(new PlaceOrderFromCartCommand(userId, request), cancellationToken);
            await distributedCache.SetStringAsync(cacheKey, string.Join(',', checkout.Orders.Select(item => item.Order.Id.ToString("D"))), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
            }, cancellationToken);

            return Ok(checkout);
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

    [HttpPost("replay/{orderId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<OrderDto>> ReplayOrderProjection(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var rebuilt = await replayOrderProjectionQueryHandler.Handle(new ReplayOrderProjectionQuery(orderId), cancellationToken);
            if (rebuilt is null)
            {
                return NotFound();
            }

            return Ok(rebuilt);
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
