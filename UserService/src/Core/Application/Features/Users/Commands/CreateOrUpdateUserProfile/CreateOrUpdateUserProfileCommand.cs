using UserService.Application.Abstractions.CQRS;
using UserService.Contracts.Dtos;

namespace UserService.Application.Features.Users.Commands.CreateOrUpdateUserProfile;

public sealed record CreateOrUpdateUserProfileCommand(CreateOrUpdateUserProfileRequestDto Request) : ICommand<UserDto?>;
