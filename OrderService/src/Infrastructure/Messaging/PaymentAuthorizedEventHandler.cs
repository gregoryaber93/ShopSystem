using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Messaging;

public sealed class PaymentAuthorizedEventHandler(
    OrderDbContext dbContext,
    ILogger<PaymentAuthorizedEventHandler> logger) : IPaymentAuthorizedEventHandler
{
    public async Task HandleAsync(Guid eventId, string payloadJson, CancellationToken cancellationToken)
    {
        var alreadyProcessed = await dbContext.OrderProcessedEvents
            .AnyAsync(processed => processed.EventId == eventId, cancellationToken);

        if (alreadyProcessed)
        {
            return;
        }

        var payload = JsonSerializer.Deserialize<PaymentAuthorizedEventPayload>(payloadJson)
            ?? throw new InvalidOperationException("Invalid payment authorized payload.");

        var order = await dbContext.Orders.FirstOrDefaultAsync(item => item.Id == payload.OrderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order '{payload.OrderId}' was not found while processing payment event '{eventId}'.");

        order.Status = ResolveOrderStatus(payload.Status);

        dbContext.OrderProcessedEvents.Add(new OrderProcessedEventEntity
        {
            EventId = eventId,
            ProcessedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Processed PaymentAuthorized event. EventId={EventId}, OrderId={OrderId}, NewOrderStatus={OrderStatus}",
            eventId,
            order.Id,
            order.Status);
    }

    private static string ResolveOrderStatus(string paymentStatus)
    {
        return paymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)
            || paymentStatus.Equals("Authorized", StringComparison.OrdinalIgnoreCase)
            ? "Paid"
            : "PaymentFailed";
    }

    private sealed record PaymentAuthorizedEventPayload(
        Guid EventId,
        DateTime OccurredOnUtc,
        Guid PaymentId,
        Guid UserId,
        Guid ShopId,
        Guid OrderId,
        decimal Amount,
        string Currency,
        string Method,
        string Status);
}
