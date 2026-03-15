namespace OrderService.Infrastructure.Messaging;

public interface IPaymentAuthorizedEventHandler
{
    Task HandleAsync(Guid eventId, string payloadJson, CancellationToken cancellationToken);
}
