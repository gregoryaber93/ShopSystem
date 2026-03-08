namespace UserService.Contracts.Dtos;

public sealed record UserDto(Guid Id, string Email, IReadOnlyCollection<string> Roles);
