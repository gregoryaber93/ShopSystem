using UserService.Application.Abstractions.CQRS;
using UserService.Contracts.Dtos;

namespace UserService.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery(string? Role) : IQuery<IReadOnlyCollection<UserDto>>;
