using AuthenticationService.Application.Abstractions.CQRS;
using AuthenticationService.Application.Features.Authentication.Commands.Login;
using AuthenticationService.Application.Features.Authentication.Commands.Register;
using AuthenticationService.Contracts.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<RegisterCommand, AuthResponseDto?>, RegisterCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, AuthResponseDto?>, LoginCommandHandler>();

        return services;
    }
}
