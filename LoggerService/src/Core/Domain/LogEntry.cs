namespace LoggerService.Domain;

public sealed class LogEntry
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
