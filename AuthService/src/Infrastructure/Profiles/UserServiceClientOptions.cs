namespace AuthService.Infrastructure.Profiles;

public sealed class UserServiceClientOptions
{
    public const string SectionName = "UserService";

    public string BaseUrl { get; init; } = "http://localhost:5101";

    public string GrpcAddress { get; init; } = "http://localhost:5101";

    public string InternalApiKey { get; init; } = "change-me";
}
