namespace LoggerService.Contracts;

public sealed record LogEntryResponse(
    Guid Id,
    string Level,
    string Message,
    string? Source,
    string? CorrelationId,
    DateTime CreatedAtUtc
);
