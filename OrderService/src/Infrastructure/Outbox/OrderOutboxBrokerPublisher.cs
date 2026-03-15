using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Messaging;
using RabbitMQ.Client;
using System.Text;

namespace OrderService.Infrastructure.Outbox;

public sealed class OrderOutboxBrokerPublisher(IOptions<MessageBrokersOptions> options) : IOrderOutboxBrokerPublisher
{
    private readonly MessageBrokersOptions _options = options.Value;

    public async Task PublishAsync(OrderOutboxMessageEntity message, CancellationToken cancellationToken)
    {
        PublishToRabbit(_options.RabbitMq.Exchange, message.EventType.ToLowerInvariant(), message);
        await PublishToKafkaAsync(message, cancellationToken);
    }

    public Task PublishDeadLetterAsync(OrderOutboxMessageEntity message, CancellationToken cancellationToken)
    {
        PublishToRabbit(_options.RabbitMq.DeadLetterExchange, $"dlq.{message.EventType.ToLowerInvariant()}", message);
        return Task.CompletedTask;
    }

    private void PublishToRabbit(string exchange, string routingKey, OrderOutboxMessageEntity message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.RabbitMq.Host,
            Port = _options.RabbitMq.Port,
            UserName = _options.RabbitMq.Username,
            Password = _options.RabbitMq.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        props.MessageId = message.EventId.ToString("N");
        props.Type = message.EventType;

        var body = Encoding.UTF8.GetBytes(message.Payload);
        channel.BasicPublish(exchange, routingKey, props, body);
    }

    private async Task PublishToKafkaAsync(OrderOutboxMessageEntity message, CancellationToken cancellationToken)
    {
        var topic = $"{_options.Kafka.TopicPrefix}.{message.EventType.ToLowerInvariant()}.v1";

        var config = new ProducerConfig
        {
            BootstrapServers = _options.Kafka.BootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All,
            MessageTimeoutMs = 5000
        };

        using var producer = new ProducerBuilder<string, string>(config).Build();
        await producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = string.IsNullOrWhiteSpace(message.PartitionKey) ? message.EventId.ToString("N") : message.PartitionKey,
            Value = message.Payload,
            Headers = new Confluent.Kafka.Headers
            {
                { "eventType", Encoding.UTF8.GetBytes(message.EventType) },
                { "eventVersion", Encoding.UTF8.GetBytes("1") },
                { "occurredOnUtc", Encoding.UTF8.GetBytes(message.OccurredOnUtc.ToString("O")) },
                { "correlationId", Encoding.UTF8.GetBytes(message.EventId.ToString("N")) }
            }
        }, cancellationToken);
    }
}
