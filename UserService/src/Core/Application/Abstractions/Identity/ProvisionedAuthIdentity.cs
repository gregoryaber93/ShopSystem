namespace UserService.Application.Abstractions.Identity;

public sealed record ProvisionedAuthIdentity(Guid Id, string Email, IReadOnlyCollection<string> Roles);