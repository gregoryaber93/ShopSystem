namespace ShopService.Contracts.Messaging;

public abstract record IntegrationEvent(Guid EventId, DateTime OccurredOnUtc);