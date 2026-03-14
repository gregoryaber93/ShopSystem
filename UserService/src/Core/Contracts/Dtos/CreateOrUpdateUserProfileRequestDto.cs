namespace UserService.Contracts.Dtos;

public sealed record CreateOrUpdateUserProfileRequestDto(Guid UserId, string Email, IReadOnlyCollection<string> Roles);
