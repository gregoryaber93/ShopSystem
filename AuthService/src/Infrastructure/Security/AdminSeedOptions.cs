namespace AuthenticationService.Infrastructure.Security;

public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Email { get; set; } = "admin@shopsystem.local";
    public string Password { get; set; } = "Admin123!";
}