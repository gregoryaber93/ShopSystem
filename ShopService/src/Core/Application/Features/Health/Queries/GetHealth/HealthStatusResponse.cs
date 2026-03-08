namespace ShopService.Application.Features.Health.Queries.GetHealth;

public sealed record HealthStatusResponse(string Status, DateTimeOffset CheckedAtUtc);
