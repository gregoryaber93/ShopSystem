using ShopService.Application.Abstractions.CQRS;

namespace ShopService.Application.Features.Health.Queries.GetHealth;

public sealed class GetHealthQueryHandler : IQueryHandler<GetHealthQuery, HealthStatusResponse>
{
    public Task<HealthStatusResponse> Handle(GetHealthQuery query, CancellationToken cancellationToken)
    {
        var response = new HealthStatusResponse("OK", DateTimeOffset.UtcNow);

        return Task.FromResult(response);
    }
}
