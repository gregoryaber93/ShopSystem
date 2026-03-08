using LoggerService.Application.Abstractions;
using LoggerService.Contracts;
using LoggerService.Domain;

namespace LoggerService.Application.Services;

public sealed class LoggingService : ILoggingService
{
    private readonly ILogStore _logStore;

    public LoggingService(ILogStore logStore)
    {
        _logStore = logStore;
    }

    public async Task<LogEntryResponse> CreateAsync(CreateLogEntryRequest request, CancellationToken cancellationToken)
    {
        var entry = new LogEntry
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

    public async Task<LogEntryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entry = await _logStore.GetByIdAsync(id, cancellationToken);
        return entry is null ? null : Map(entry);
    }

    public async Task<IReadOnlyCollection<LogEntryResponse>> GetLatestAsync(int take, CancellationToken cancellationToken)
    {
        var entries = await _logStore.GetLatestAsync(take, cancellationToken);
        return entries.Select(Map).ToArray();
    }

    private static LogEntryResponse Map(LogEntry entry)
    {
        return new LogEntryResponse(
            entry.Id,
            entry.Level,
            entry.Message,
            entry.Source,
            entry.CorrelationId,
            entry.CreatedAtUtc
        );
    }
}
