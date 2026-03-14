namespace AuthService.Infrastructure.Observability;

public interface ILoggerServiceClient
{
    Task SendAsync(string source, string message, string? correlationId, CancellationToken cancellationToken = default);
}
