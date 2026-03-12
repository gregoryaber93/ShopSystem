using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopService.Application.Abstractions.Messaging;
using ShopService.Contracts.Messaging;
using System.Collections.Concurrent;

namespace ShopService.Infrastructure.Messaging;

internal sealed class CompositeEventPublisher(
    RabbitMqEventPublisher rabbitMqEventPublisher,
    KafkaEventPublisher kafkaEventPublisher,
    IOptions<MessageBrokersOptions> options,
    ILogger<CompositeEventPublisher> logger) : IEventPublisher
{
    private readonly MessageBrokersOptions _options = options.Value;
    private static readonly ConcurrentDictionary<Guid, byte> ProcessedEventIds = new();

    public async Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var eventId = eventMessage is IntegrationEvent integrationEvent
            ? integrationEvent.EventId
            : Guid.NewGuid();

        if (!ProcessedEventIds.TryAdd(eventId, 0))
        {
            logger.LogWarning("Skipping duplicate event publish attempt for EventId {EventId}", eventId);
            return;
        }

        await rabbitMqEventPublisher.PublishAsync(eventMessage, eventId.ToString("N"), cancellationToken);
        await kafkaEventPublisher.PublishAsync(eventMessage, eventId.ToString("N"), cancellationToken);

        logger.LogInformation(
            "Published integration event {EventType} ({EventId}) via RabbitMQ ({RabbitMqHost}) and Kafka ({KafkaBootstrapServers}).",
            typeof(TEvent).Name,
            eventId,
            _options.RabbitMq.Host,
            _options.Kafka.BootstrapServers);
    }
}