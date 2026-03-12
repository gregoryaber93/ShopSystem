using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopService.Infrastructure.Security;
using System.Text.Json;

namespace ShopService.Infrastructure.Messaging;

internal sealed class KafkaEventPublisher(
    IOptions<MessageBrokersOptions> options,
    IJwtTokenService jwtTokenService,
    ILogger<KafkaEventPublisher> logger)
{
    private readonly MessageBrokersOptions _options = options.Value;

    public async Task PublishAsync<TEvent>(TEvent eventMessage, string eventId, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var jwtToken = jwtTokenService.CreateServiceToken("shopservice-kafka-publisher");
        var eventType = typeof(TEvent).Name;
        var topic = $"{_options.Kafka.TopicPrefix}.{eventType.ToLowerInvariant()}.v1";

        var config = new ProducerConfig
        {
            BootstrapServers = _options.Kafka.BootstrapServers,
            MessageTimeoutMs = 5000,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        using var producer = new ProducerBuilder<string, string>(config)
            .SetValueSerializer(Serializers.Utf8)
            .Build();

        var jsonPayload = JsonSerializer.Serialize(eventMessage);
        var message = new Message<string, string>
        {
            Key = eventId,
            Value = jsonPayload,
            Headers = new Headers
            {
                { "Authorization", System.Text.Encoding.UTF8.GetBytes($"Bearer {jwtToken}") },
                { "eventType", System.Text.Encoding.UTF8.GetBytes(eventType) }
            }
        };

        await producer.ProduceAsync(topic, message, cancellationToken);

        logger.LogDebug(
            "Kafka event published. EventType: {EventType}, EventId: {EventId}, BootstrapServers: {BootstrapServers}, Topic: {Topic}",
            eventType,
            eventId,
            _options.Kafka.BootstrapServers,
            topic);
    }
}