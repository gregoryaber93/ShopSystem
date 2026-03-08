namespace DashboardService.Contracts;

public sealed record DashboardItemResponse(
    Guid Id,
    string Level,
    string Message,
    string? Source,
    string? CorrelationId,
    DateTime CreatedAtUtc
);


