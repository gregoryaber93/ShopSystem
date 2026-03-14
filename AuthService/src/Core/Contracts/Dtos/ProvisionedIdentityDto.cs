namespace AuthenticationService.Contracts.Dtos;

public sealed record ProvisionedIdentityDto(Guid Id, string Email, IReadOnlyCollection<string> Roles);