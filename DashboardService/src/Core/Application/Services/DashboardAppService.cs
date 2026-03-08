using DashboardService.Application.Abstractions;
using DashboardService.Contracts;
using DashboardService.Domain;

namespace DashboardService.Application.Services;

public sealed class DashboardAppService : IDashboardService
{
    private readonly IDashboardStore _logStore;

    public DashboardAppService(IDashboardStore logStore)
    {
        _logStore = logStore;
    }

    public async Task<DashboardItemResponse> CreateAsync(CreateDashboardItemRequest request, CancellationToken cancellationToken)
    {
        var entry = new DashboardItem
        {
            Id = Guid.NewGuid(),
            Level = string.IsNullOrWhiteSpace(request.Level) ? "Information" : request.Level.Trim(),
            Message = request.Message.Trim(),
            Source = request.Source?.Trim(),
            CorrelationId = request.CorrelationId?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _logStore.AddAsync(entry, cancellationToken);
        return Map(entry);
    }

    public async Task<DashboardItemResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entry = await _logStore.GetByIdAsync(id, cancellationToken);
        return entry is null ? null : Map(entry);
    }

    public async Task<IReadOnlyCollection<DashboardItemResponse>> GetLatestAsync(int take, CancellationToken cancellationToken)
    {
        var entries = await _logStore.GetLatestAsync(take, cancellationToken);
        return entries.Select(Map).ToArray();
    }

    private static DashboardItemResponse Map(DashboardItem entry)
    {
        return new DashboardItemResponse(
            entry.Id,
            entry.Level,
            entry.Message,
            entry.Source,
            entry.CorrelationId,
            entry.CreatedAtUtc
        );
    }
}


