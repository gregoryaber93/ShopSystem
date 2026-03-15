using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence;
using System.Text.Json;

namespace OrderService.Infrastructure.Tests;

public class PaymentAuthorizedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenAlreadyProcessed_DoesNotUpdateOrderAgain()
    {
        var dbContext = CreateDbContext();
        var eventId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        dbContext.Orders.Add(new OrderEntity
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            ShopId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 1,
            UnitPrice = 100m,
            TotalPrice = 100m,
            OrderedAtUtc = DateTime.UtcNow,
            Status = "Placed"
        });

        dbContext.OrderProcessedEvents.Add(new OrderProcessedEventEntity
        {
            EventId = eventId,
            ProcessedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var handler = new PaymentAuthorizedEventHandler(dbContext, NullLogger<PaymentAuthorizedEventHandler>.Instance);
        await handler.HandleAsync(eventId, CreatePayload(orderId, "Completed"), CancellationToken.None);

        var order = await dbContext.Orders.SingleAsync(item => item.Id == orderId);
        Assert.Equal("Placed", order.Status);
        Assert.Equal(1, await dbContext.OrderProcessedEvents.CountAsync(item => item.EventId == eventId));
    }

    [Fact]
    public async Task HandleAsync_WhenNewEvent_UpdatesOrderAndMarksEventProcessed()
    {
        var dbContext = CreateDbContext();
        var eventId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        dbContext.Orders.Add(new OrderEntity
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            ShopId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 2,
            UnitPrice = 50m,
            TotalPrice = 100m,
            OrderedAtUtc = DateTime.UtcNow,
            Status = "Placed"
        });

        await dbContext.SaveChangesAsync();

        var handler = new PaymentAuthorizedEventHandler(dbContext, NullLogger<PaymentAuthorizedEventHandler>.Instance);
        await handler.HandleAsync(eventId, CreatePayload(orderId, "Completed"), CancellationToken.None);

        var order = await dbContext.Orders.SingleAsync(item => item.Id == orderId);
        Assert.Equal("Paid", order.Status);
        Assert.True(await dbContext.OrderProcessedEvents.AnyAsync(item => item.EventId == eventId));
    }

    [Fact]
    public async Task HandleAsync_WhenPaymentStatusIsFailed_MapsOrderToPaymentFailed()
    {
        var dbContext = CreateDbContext();
        var eventId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        dbContext.Orders.Add(new OrderEntity
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            ShopId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 1,
            UnitPrice = 75m,
            TotalPrice = 75m,
            OrderedAtUtc = DateTime.UtcNow,
            Status = "Placed"
        });

        await dbContext.SaveChangesAsync();

        var handler = new PaymentAuthorizedEventHandler(dbContext, NullLogger<PaymentAuthorizedEventHandler>.Instance);
        await handler.HandleAsync(eventId, CreatePayload(orderId, "Failed"), CancellationToken.None);

        var order = await dbContext.Orders.SingleAsync(item => item.Id == orderId);
        Assert.Equal("PaymentFailed", order.Status);
        Assert.True(await dbContext.OrderProcessedEvents.AnyAsync(item => item.EventId == eventId));
    }

    [Fact]
    public async Task HandleAsync_WhenOrderMissing_ThrowsToTriggerRetry()
    {
        var dbContext = CreateDbContext();
        var eventId = Guid.NewGuid();

        var handler = new PaymentAuthorizedEventHandler(dbContext, NullLogger<PaymentAuthorizedEventHandler>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(eventId, CreatePayload(Guid.NewGuid(), "Completed"), CancellationToken.None));

        Assert.False(await dbContext.OrderProcessedEvents.AnyAsync(item => item.EventId == eventId));
    }

    private static OrderDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase($"order-tests-{Guid.NewGuid():N}")
            .Options;

        return new OrderDbContext(options);
    }

    private static string CreatePayload(Guid orderId, string status)
    {
        return JsonSerializer.Serialize(new
        {
            EventId = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            PaymentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ShopId = Guid.NewGuid(),
            OrderId = orderId,
            Amount = 120.50m,
            Currency = "PLN",
            Method = "Card",
            Status = status
        });
    }
}
