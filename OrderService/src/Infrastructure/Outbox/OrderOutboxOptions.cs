namespace OrderService.Infrastructure.Outbox;

public sealed class OrderOutboxOptions
{
    public const string SectionName = "Outbox";

    public int BatchSize { get; set; } = 50;

    public int PollIntervalSeconds { get; set; } = 5;

    public int MaxRetries { get; set; } = 5;
}
