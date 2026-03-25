using UserService.Application.Abstractions.CQRS;
using UserService.Application.Abstractions.Persistence;
using UserService.Contracts.Dtos;

namespace UserService.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetUsersQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetUsersWithRolesAsync(query.Role, cancellationToken);

        return users
            .Select(user => new UserDto(
                user.Id,
                user.Email,
                user.UserRoles
                    .Select(userRole => userRole.Role.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()))
            .ToArray();
    }
}
