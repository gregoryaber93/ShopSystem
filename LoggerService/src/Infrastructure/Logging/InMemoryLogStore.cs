using System.Collections.Concurrent;
using LoggerService.Application.Abstractions;
using LoggerService.Domain;

namespace LoggerService.Infrastructure.Logging;

public sealed class InMemoryLogStore : ILogStore
{
    private readonly ConcurrentDictionary<Guid, LogEntry> _entriesById = new();
    private readonly ConcurrentQueue<Guid> _order = new();

    public Task AddAsync(LogEntry entry, CancellationToken cancellationToken)
    {
        _entriesById[entry.Id] = entry;
        _order.Enqueue(entry.Id);
        return Task.CompletedTask;
    }

    public Task<LogEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _entriesById.TryGetValue(id, out var entry);
        return Task.FromResult(entry);
    }

    public Task<IReadOnlyCollection<LogEntry>> GetLatestAsync(int take, CancellationToken cancellationToken)
    {
        var ids = _order.ToArray();
        var result = new List<LogEntry>(capacity: Math.Min(take, ids.Length));

        for (var index = ids.Length - 1; index >= 0 && result.Count < take; index--)
        {
            if (_entriesById.TryGetValue(ids[index], out var entry))
            {
                result.Add(entry);
            }
        }

        return Task.FromResult<IReadOnlyCollection<LogEntry>>(result);
    }
}
