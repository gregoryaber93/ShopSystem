namespace DashboardService.Contracts;

public sealed record CreateDashboardItemRequest(
    string Level,
    string Message,
    string? Source,
    string? CorrelationId
);


