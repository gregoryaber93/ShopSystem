using AuthenticationService.Application.Abstractions.Persistence;
using AuthenticationService.Application.Abstractions.Security;
using AuthenticationService.Infrastructure.Persistence;
using AuthenticationService.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not found.");

        services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString));

        services.Configure<JwtRsaOptions>(configuration.GetSection(JwtRsaOptions.SectionName));

        services.AddScoped<IAuthUserRepository, AuthUserRepository>();
        services.AddSingleton<IPasswordHasherService, Pbkdf2PasswordHasherService>();
        services.AddSingleton<IJwtTokenService, RsaJwtTokenService>();

        return services;
    }
}
