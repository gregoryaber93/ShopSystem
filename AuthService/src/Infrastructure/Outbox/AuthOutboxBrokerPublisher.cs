using System.Text;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AuthenticationService.Infrastructure.Outbox;

public sealed class AuthOutboxBrokerPublisher(IOptions<MessageBrokersOptions> options) : IAuthOutboxBrokerPublisher
{
    private readonly MessageBrokersOptions brokerOptions = options.Value;

    public Task PublishAsync(AuthOutboxMessageEntity message, CancellationToken cancellationToken)
    {
        PublishToRabbit(brokerOptions.RabbitMq.Exchange, brokerOptions.RabbitMq.UserCreatedRoutingKey, message);
        return Task.CompletedTask;
    }

    public Task PublishDeadLetterAsync(AuthOutboxMessageEntity message, CancellationToken cancellationToken)
    {
        PublishToRabbit(brokerOptions.RabbitMq.DeadLetterExchange, $"dlq.{brokerOptions.RabbitMq.UserCreatedRoutingKey}", message);
        return Task.CompletedTask;
    }

    private void PublishToRabbit(string exchange, string routingKey, AuthOutboxMessageEntity message)
    {
        var factory = new ConnectionFactory
        {
            HostName = brokerOptions.RabbitMq.Host,
            Port = brokerOptions.RabbitMq.Port,
            UserName = brokerOptions.RabbitMq.Username,
            Password = brokerOptions.RabbitMq.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Type = message.EventType;
        properties.MessageId = message.Id.ToString("N");

        var body = Encoding.UTF8.GetBytes(message.PayloadJson);
        channel.BasicPublish(exchange, routingKey, properties, body);
    }
}
