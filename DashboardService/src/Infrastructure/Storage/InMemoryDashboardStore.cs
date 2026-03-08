using System.Collections.Concurrent;
using DashboardService.Application.Abstractions;
using DashboardService.Domain;

namespace DashboardService.Infrastructure.Storage;

public sealed class InMemoryDashboardStore : IDashboardStore
{
    private readonly ConcurrentDictionary<Guid, DashboardItem> _entriesById = new();
    private readonly ConcurrentQueue<Guid> _order = new();

    public Task AddAsync(DashboardItem entry, CancellationToken cancellationToken)
    {
        _entriesById[entry.Id] = entry;
        _order.Enqueue(entry.Id);
        return Task.CompletedTask;
    }

    public Task<DashboardItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _entriesById.TryGetValue(id, out var entry);
        return Task.FromResult(entry);
    }

    public Task<IReadOnlyCollection<DashboardItem>> GetLatestAsync(int take, CancellationToken cancellationToken)
    {
        var ids = _order.ToArray();
        var result = new List<DashboardItem>(capacity: Math.Min(take, ids.Length));

        for (var index = ids.Length - 1; index >= 0 && result.Count < take; index--)
        {
            if (_entriesById.TryGetValue(ids[index], out var entry))
            {
                result.Add(entry);
            }
        }

        return Task.FromResult<IReadOnlyCollection<DashboardItem>>(result);
    }
}


