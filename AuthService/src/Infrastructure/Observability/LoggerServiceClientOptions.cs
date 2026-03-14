namespace AuthenticationService.Infrastructure.Observability;

public sealed class LoggerServiceClientOptions
{
    public const string SectionName = "LoggerService";
    public string BaseUrl { get; init; } = "http://localhost:5300";
}
