using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Abstractions.CQRS;
using UserService.Application.Features.Users.Commands.CreateOrUpdateUserProfile;
using UserService.Application.Features.Users.Commands.CreateUser;
using UserService.Application.Features.Users.Commands.DeleteUser;
using UserService.Application.Features.Users.Commands.UpdateUser;
using UserService.Contracts.Dtos;

namespace UserService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateUserCommand, UserDto?>, CreateUserCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateUserCommand, UserDto?>, UpdateUserCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteUserCommand, bool>, DeleteUserCommandHandler>();
        services.AddScoped<ICommandHandler<CreateOrUpdateUserProfileCommand, UserDto?>, CreateOrUpdateUserProfileCommandHandler>();
        return services;
    }
}
