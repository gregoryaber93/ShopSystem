using DashboardService.Domain;

namespace DashboardService.Application.Abstractions;

public interface IDashboardStore
{
    Task AddAsync(DashboardItem entry, CancellationToken cancellationToken);
    Task<DashboardItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DashboardItem>> GetLatestAsync(int take, CancellationToken cancellationToken);
}


