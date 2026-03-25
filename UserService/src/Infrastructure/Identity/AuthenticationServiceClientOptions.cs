namespace UserService.Infrastructure.Identity;

public sealed class AuthenticationServiceClientOptions
{
    public const string SectionName = "AuthenticationService";

    public string BaseUrl { get; set; } = "http://localhost:5294";
    public string GrpcAddress { get; set; } = "http://localhost:5295";
}