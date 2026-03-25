using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymantService.Application.Abstractions.CQRS;
using PaymantService.Application.Features.Payments.Commands.ProcessPayment;
using PaymantService.Application.Features.Payments.Queries.GetMyPayments;
using PaymantService.Application.Features.Payments.Queries.GetPaymentsByShop;
using PaymantService.Contracts.Dtos;

namespace PaymantService.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Roles = "Admin,Manager,User")]
public class PaymentsController(
    ICommandHandler<ProcessPaymentCommand, PaymentDto> processPaymentCommandHandler,
    IDistributedCache distributedCache,
    IQueryHandler<GetMyPaymentsQuery, IReadOnlyCollection<PaymentDto>> getMyPaymentsQueryHandler,
    IQueryHandler<GetPaymentsByShopQuery, IReadOnlyCollection<PaymentDto>> getPaymentsByShopQueryHandler) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<PaymentDto>> ProcessPayment([FromBody] ProcessPaymentRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var idempotencyKey = Request.Headers["Idempotency-Key"].ToString();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return BadRequest(new { error = "Idempotency-Key header is required." });
            }

            var userId = ResolveUserId(User);
            var cacheKey = $"idempotency:payments:{userId:N}:{idempotencyKey}";
            var existingPaymentId = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(existingPaymentId) && Guid.TryParse(existingPaymentId, out var parsedPaymentId))
            {
                return Ok(new { paymentId = parsedPaymentId, duplicate = true });
            }

            var payment = await processPaymentCommandHandler.Handle(new ProcessPaymentCommand(userId, request), cancellationToken);
            await distributedCache.SetStringAsync(cacheKey, payment.Id.ToString("D"), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
            }, cancellationToken);

            return Created($"/api/payments/{payment.Id}", payment);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("my")]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<IReadOnlyCollection<PaymentDto>>> GetMyPayments(CancellationToken cancellationToken)
    {
        try
        {
            var userId = ResolveUserId(User);
            var payments = await getMyPaymentsQueryHandler.Handle(new GetMyPaymentsQuery(userId), cancellationToken);
            return Ok(payments);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("shop/{shopId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IReadOnlyCollection<PaymentDto>>> GetPaymentsByShop(Guid shopId, CancellationToken cancellationToken)
    {
        try
        {
            var payments = await getPaymentsByShopQueryHandler.Handle(new GetPaymentsByShopQuery(shopId), cancellationToken);
            return Ok(payments);
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


