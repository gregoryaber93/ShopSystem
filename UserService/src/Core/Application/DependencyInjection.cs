using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Abstractions.CQRS;
using UserService.Application.Features.Users.Commands.CreateOrUpdateUserProfile;
using UserService.Application.Features.Users.Commands.CreateUser;
using UserService.Contracts.Dtos;

namespace UserService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateUserCommand, UserDto?>, CreateUserCommandHandler>();
        services.AddScoped<ICommandHandler<CreateOrUpdateUserProfileCommand, UserDto?>, CreateOrUpdateUserProfileCommandHandler>();
        return services;
    }
}
