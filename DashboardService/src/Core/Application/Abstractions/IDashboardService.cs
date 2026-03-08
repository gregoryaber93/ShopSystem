using DashboardService.Contracts;

namespace DashboardService.Application.Abstractions;

public interface IDashboardService
{
    Task<DashboardItemResponse> CreateAsync(CreateDashboardItemRequest request, CancellationToken cancellationToken);
    Task<DashboardItemResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DashboardItemResponse>> GetLatestAsync(int take, CancellationToken cancellationToken);
}


