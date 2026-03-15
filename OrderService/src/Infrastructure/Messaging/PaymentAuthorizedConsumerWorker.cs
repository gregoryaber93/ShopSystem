using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderService.Infrastructure.Messaging;

public sealed class PaymentAuthorizedConsumerWorker(
    IOptions<MessageBrokersOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<PaymentAuthorizedConsumerWorker> logger) : BackgroundService
{
    private readonly MessageBrokersOptions brokerOptions = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = brokerOptions.RabbitMq.Host,
            Port = brokerOptions.RabbitMq.Port,
            UserName = brokerOptions.RabbitMq.Username,
            Password = brokerOptions.RabbitMq.Password,
            DispatchConsumersAsync = true
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(brokerOptions.RabbitMq.PaymentEventsExchange, ExchangeType.Topic, durable: true);
        channel.QueueDeclare(
            brokerOptions.RabbitMq.PaymentAuthorizedQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);
        channel.QueueBind(
            brokerOptions.RabbitMq.PaymentAuthorizedQueue,
            brokerOptions.RabbitMq.PaymentEventsExchange,
            brokerOptions.RabbitMq.PaymentAuthorizedRoutingKey);
        channel.QueueBind(
            brokerOptions.RabbitMq.PaymentAuthorizedQueue,
            brokerOptions.RabbitMq.PaymentEventsExchange,
            brokerOptions.RabbitMq.PaymentFailedRoutingKey);
        channel.BasicQos(0, 10, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, eventArgs) => { await HandleMessageAsync(channel, eventArgs, stoppingToken); };

        channel.BasicConsume(brokerOptions.RabbitMq.PaymentAuthorizedQueue, autoAck: false, consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task HandleMessageAsync(IModel channel, BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var eventId = ParseEventId(eventArgs.BasicProperties.MessageId);

        try
        {
            var payloadJson = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            using var scope = scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IPaymentAuthorizedEventHandler>();
            await handler.HandleAsync(eventId, payloadJson, cancellationToken);

            channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to process payment event. EventId={EventId}, Queue={Queue}",
                eventId,
                brokerOptions.RabbitMq.PaymentAuthorizedQueue);

            channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
        }
    }

    private static Guid ParseEventId(string? messageId)
    {
        return Guid.TryParse(messageId, out var parsed) ? parsed : Guid.NewGuid();
    }
}
