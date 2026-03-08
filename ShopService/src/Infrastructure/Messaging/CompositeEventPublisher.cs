using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopService.Application.Abstractions.Messaging;

namespace ShopService.Infrastructure.Messaging;

internal sealed class CompositeEventPublisher(
    RabbitMqEventPublisher rabbitMqEventPublisher,
    KafkaEventPublisher kafkaEventPublisher,
    IOptions<MessageBrokersOptions> options,
    ILogger<CompositeEventPublisher> logger) : IEventPublisher
{
    private readonly MessageBrokersOptions _options = options.Value;

    public async Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        await rabbitMqEventPublisher.PublishAsync(eventMessage, cancellationToken);
        await kafkaEventPublisher.PublishAsync(eventMessage, cancellationToken);

        logger.LogInformation(
            "Published integration event {EventType} via RabbitMQ ({RabbitMqHost}) and Kafka ({KafkaBootstrapServers}).",
            typeof(TEvent).Name,
            _options.RabbitMq.Host,
            _options.Kafka.BootstrapServers);
    }
}