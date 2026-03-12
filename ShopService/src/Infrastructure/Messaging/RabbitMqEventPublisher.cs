using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ShopService.Infrastructure.Security;
using System.Text;
using System.Text.Json;

namespace ShopService.Infrastructure.Messaging;

internal sealed class RabbitMqEventPublisher(
    IOptions<MessageBrokersOptions> options,
    IJwtTokenService jwtTokenService,
    ILogger<RabbitMqEventPublisher> logger)
{
    private readonly MessageBrokersOptions _options = options.Value;

    public Task PublishAsync<TEvent>(TEvent eventMessage, string eventId, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var jwtToken = jwtTokenService.CreateServiceToken("shopservice-rabbitmq-publisher");
        var eventType = typeof(TEvent).Name;
        var routingKey = eventType.ToLowerInvariant();

        var factory = new ConnectionFactory
        {
            HostName = _options.RabbitMq.Host,
            Port = _options.RabbitMq.Port,
            UserName = _options.RabbitMq.Username,
            Password = _options.RabbitMq.Password,
            DispatchConsumersAsync = true
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(_options.RabbitMq.Exchange, ExchangeType.Topic, durable: true);
        channel.ExchangeDeclare(_options.RabbitMq.DeadLetterExchange, ExchangeType.Topic, durable: true);

        var queueName = $"{routingKey}.queue";
        var retryQueueName = $"{routingKey}.retry.queue";
        var dlqName = $"{routingKey}.dlq";

        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = _options.RabbitMq.DeadLetterExchange,
            ["x-dead-letter-routing-key"] = $"dlq.{routingKey}"
        });
        channel.QueueBind(queueName, _options.RabbitMq.Exchange, routingKey);

        channel.QueueDeclare(retryQueueName, durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object>
        {
            ["x-message-ttl"] = _options.RabbitMq.RetryDelayMilliseconds,
            ["x-dead-letter-exchange"] = _options.RabbitMq.Exchange,
            ["x-dead-letter-routing-key"] = routingKey
        });

        channel.QueueDeclare(dlqName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind(dlqName, _options.RabbitMq.DeadLetterExchange, $"dlq.{routingKey}");

        var payload = JsonSerializer.SerializeToUtf8Bytes(eventMessage);
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = eventId;
        properties.CorrelationId = eventId;
        properties.Type = eventType;
        properties.Headers = new Dictionary<string, object>
        {
            ["Authorization"] = Encoding.UTF8.GetBytes($"Bearer {jwtToken}")
        };

        channel.BasicPublish(
            exchange: _options.RabbitMq.Exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: payload);

        logger.LogDebug(
            "RabbitMQ event published. EventType: {EventType}, EventId: {EventId}, Host: {Host}, Exchange: {Exchange}",
            eventType,
            eventId,
            _options.RabbitMq.Host,
            _options.RabbitMq.Exchange);

        return Task.CompletedTask;
    }
}