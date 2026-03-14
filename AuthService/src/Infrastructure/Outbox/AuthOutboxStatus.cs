namespace AuthService.Infrastructure.Outbox;

internal static class AuthOutboxStatus
{
    public const string Pending = "Pending";
    public const string Completed = "Completed";
    public const string FailedRetryable = "FailedRetryable";
    public const string FailedPermanent = "FailedPermanent";
}
