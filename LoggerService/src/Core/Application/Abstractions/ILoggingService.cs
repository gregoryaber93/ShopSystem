using LoggerService.Contracts;

namespace LoggerService.Application.Abstractions;

public interface ILoggingService
{
    Task<LogEntryResponse> CreateAsync(CreateLogEntryRequest request, CancellationToken cancellationToken);
    Task<LogEntryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LogEntryResponse>> GetLatestAsync(int take, CancellationToken cancellationToken);
}
