namespace AuthenticationService.Infrastructure.Outbox;

public sealed class AuthOutboxOptions
{
    public const string SectionName = "Outbox";

    public int PollIntervalSeconds { get; set; } = 10;

    public int BatchSize { get; set; } = 25;

    public int MaxRetries { get; set; } = 10;
}
