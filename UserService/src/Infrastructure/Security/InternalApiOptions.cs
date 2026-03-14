namespace UserService.Infrastructure.Security;

public sealed class InternalApiOptions
{
    public const string SectionName = "InternalApi";

    public string ApiKey { get; init; } = "change-me";
}
