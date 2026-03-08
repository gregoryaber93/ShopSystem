namespace ShopService.Application.Abstractions.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class;
}