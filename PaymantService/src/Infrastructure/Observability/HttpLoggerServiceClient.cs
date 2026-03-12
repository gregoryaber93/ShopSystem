using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace PaymantService.Infrastructure.Observability;

public sealed class HttpLoggerServiceClient(
    HttpClient httpClient,
    ILogger<HttpLoggerServiceClient> logger) : ILoggerServiceClient
{
    public async Task SendAsync(string source, string message, string? correlationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { level = "Error", message, source, correlationId };
            await httpClient.PostAsJsonAsync("api/logs", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send log to LoggerService (fallback to local logger). Source={Source}", source);
        }
    }
}
