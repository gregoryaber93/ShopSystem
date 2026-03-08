using ShopService.Application.Abstractions.CQRS;

namespace ShopService.Application.Features.Health.Queries.GetHealth;

public sealed record GetHealthQuery : IQuery<HealthStatusResponse>;
