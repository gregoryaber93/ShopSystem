using LoggerService.Domain;

namespace LoggerService.Application.Abstractions;

public interface ILogStore
{
    Task AddAsync(LogEntry entry, CancellationToken cancellationToken);
    Task<LogEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LogEntry>> GetLatestAsync(int take, CancellationToken cancellationToken);
}
