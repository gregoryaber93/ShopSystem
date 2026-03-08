namespace DashboardService.Domain;

public sealed class DashboardItem
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}


