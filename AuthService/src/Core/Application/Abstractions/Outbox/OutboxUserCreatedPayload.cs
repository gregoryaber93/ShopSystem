namespace AuthenticationService.Application.Abstractions.Outbox;

public sealed record OutboxUserCreatedPayload(Guid UserId, string Email, IReadOnlyCollection<string> Roles);
