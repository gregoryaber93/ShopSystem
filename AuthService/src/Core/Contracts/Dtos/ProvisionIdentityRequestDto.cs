namespace AuthenticationService.Contracts.Dtos;

public sealed record ProvisionIdentityRequestDto(string Email, string Password, IReadOnlyCollection<string> Roles);