using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UserService.Application.Abstractions.CQRS;
using UserService.Application.Features.Users.Commands.CreateOrUpdateUserProfile;
using UserService.Contracts.Dtos;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Messaging;

public sealed class UserCreatedConsumerWorker(
    IOptions<MessageBrokersOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<UserCreatedConsumerWorker> logger) : BackgroundService
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

        channel.ExchangeDeclare(brokerOptions.RabbitMq.Exchange, ExchangeType.Topic, durable: true);
        channel.QueueDeclare(brokerOptions.RabbitMq.UserCreatedQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(brokerOptions.RabbitMq.UserCreatedQueue, brokerOptions.RabbitMq.Exchange, brokerOptions.RabbitMq.UserCreatedRoutingKey);
        channel.BasicQos(0, 10, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, eventArgs) =>
        {
            await HandleMessageAsync(channel, eventArgs, stoppingToken);
        };

        channel.BasicConsume(brokerOptions.RabbitMq.UserCreatedQueue, autoAck: false, consumer);

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
            var payload = JsonSerializer.Deserialize<UserCreatedEventPayload>(payloadJson)
                ?? throw new InvalidOperationException("Invalid user created payload.");

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            var commandHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateOrUpdateUserProfileCommand, UserDto?>>();

            var alreadyProcessed = await dbContext.ProcessedEvents
                .AnyAsync(processed => processed.EventId == eventId, cancellationToken);

            if (alreadyProcessed)
            {
                channel.BasicAck(eventArgs.DeliveryTag, false);
                return;
            }

            var request = new CreateOrUpdateUserProfileRequestDto(payload.UserId, payload.Email, payload.Roles);
            var result = await commandHandler.Handle(new CreateOrUpdateUserProfileCommand(request), cancellationToken);

            if (result is null)
            {
                throw new InvalidOperationException("User profile conflict detected while consuming UserCreated event.");
            }

            dbContext.ProcessedEvents.Add(new UserProcessedEventEntity
            {
                EventId = eventId,
                ProcessedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            channel.BasicAck(eventArgs.DeliveryTag, false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to process UserCreated event. EventId={EventId}", eventId);
            channel.BasicNack(eventArgs.DeliveryTag, false, requeue: true);
        }
    }

    private static Guid ParseEventId(string? messageId)
    {
        return Guid.TryParse(messageId, out var parsed) ? parsed : Guid.NewGuid();
    }

    private sealed record UserCreatedEventPayload(Guid UserId, string Email, string[] Roles);
}
