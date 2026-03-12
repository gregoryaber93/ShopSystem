namespace PaymantService.Infrastructure.Outbox;

public sealed class PaymentOutboxOptions
{
    public const string SectionName = "Outbox";

    public int BatchSize { get; set; } = 50;

    public int PollIntervalSeconds { get; set; } = 5;

    public int MaxRetries { get; set; } = 5;
}
