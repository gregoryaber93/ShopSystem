namespace UserService.Contracts.Dtos;

public sealed record CreateUserRequestDto(string Email, string Password, IReadOnlyCollection<string> Roles);
