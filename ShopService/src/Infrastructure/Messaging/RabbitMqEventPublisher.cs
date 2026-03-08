using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopService.Infrastructure.Security;

namespace ShopService.Infrastructure.Messaging;

internal sealed class RabbitMqEventPublisher(
    IOptions<MessageBrokersOptions> options,
    IJwtTokenService jwtTokenService,
    ILogger<RabbitMqEventPublisher> logger)
{
    private readonly MessageBrokersOptions _options = options.Value;

    public Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var jwtToken = jwtTokenService.CreateServiceToken("shopservice-rabbitmq-publisher");

        logger.LogDebug(
            "RabbitMQ event prepared for publishing. EventType: {EventType}, Host: {Host}, Exchange: {Exchange}, AuthHeaderAttached: {AuthHeaderAttached}",
            typeof(TEvent).Name,
            _options.RabbitMq.Host,
            _options.RabbitMq.Exchange,
            !string.IsNullOrWhiteSpace(jwtToken));

        return Task.CompletedTask;
    }
}