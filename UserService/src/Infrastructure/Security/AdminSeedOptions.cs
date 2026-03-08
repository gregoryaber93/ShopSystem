namespace UserService.Infrastructure.Security;

public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Email { get; init; } = "admin@shopsystem.local";
    public string Password { get; init; } = "Admin123!";
}
