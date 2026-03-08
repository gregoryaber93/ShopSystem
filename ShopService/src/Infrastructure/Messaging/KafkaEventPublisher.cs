using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopService.Infrastructure.Security;

namespace ShopService.Infrastructure.Messaging;

internal sealed class KafkaEventPublisher(
    IOptions<MessageBrokersOptions> options,
    IJwtTokenService jwtTokenService,
    ILogger<KafkaEventPublisher> logger)
{
    private readonly MessageBrokersOptions _options = options.Value;

    public Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var jwtToken = jwtTokenService.CreateServiceToken("shopservice-kafka-publisher");

        logger.LogDebug(
            "Kafka event prepared for publishing. EventType: {EventType}, BootstrapServers: {BootstrapServers}, TopicPrefix: {TopicPrefix}, AuthHeaderAttached: {AuthHeaderAttached}",
            typeof(TEvent).Name,
            _options.Kafka.BootstrapServers,
            _options.Kafka.TopicPrefix,
            !string.IsNullOrWhiteSpace(jwtToken));

        return Task.CompletedTask;
    }
}