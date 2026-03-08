using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Abstractions.Persistence;
using UserService.Application.Abstractions.Security;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Security;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not found.");

        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<JwtRsaOptions>(configuration.GetSection(JwtRsaOptions.SectionName));
        services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasherService, Pbkdf2PasswordHasherService>();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

        return services;
    }
}
