using AuthService.Application.Abstractions.CQRS;
using AuthService.Application.Features.Authentication.Commands.DeleteIdentity;
using AuthService.Application.Features.Authentication.Commands.Login;
using AuthService.Application.Features.Authentication.Commands.ProvisionIdentity;
using AuthService.Application.Features.Authentication.Commands.Register;
using AuthService.Application.Features.Authentication.Commands.UpdateIdentity;
using AuthService.Contracts.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<RegisterCommand, AuthResponseDto?>, RegisterCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, AuthResponseDto?>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<ProvisionIdentityCommand, ProvisionedIdentityDto?>, ProvisionIdentityCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateIdentityCommand, ProvisionedIdentityDto?>, UpdateIdentityCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteIdentityCommand, bool>, DeleteIdentityCommandHandler>();

        return services;
    }
}
