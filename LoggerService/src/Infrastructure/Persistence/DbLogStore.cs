using LoggerService.Application.Abstractions;
using LoggerService.Domain;
using Microsoft.EntityFrameworkCore;

namespace LoggerService.Infrastructure.Persistence;

public sealed class DbLogStore(LoggerDbContext dbContext) : ILogStore
{
    public async Task AddAsync(LogEntry entry, CancellationToken cancellationToken)
    {
        await dbContext.LogEntries.AddAsync(entry, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<LogEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.LogEntries.FirstOrDefaultAsync(log => log.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<LogEntry>> GetLatestAsync(int take, CancellationToken cancellationToken)
    {
        return await dbContext.LogEntries
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
