namespace LoggerService.Contracts;

public sealed record CreateLogEntryRequest(
    string Level,
    string Message,
    string? Source,
    string? CorrelationId
);
