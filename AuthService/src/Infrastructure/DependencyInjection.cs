using AuthService.Application.Abstractions.Outbox;
using AuthService.Application.Abstractions.Persistence;
using AuthService.Application.Abstractions.Profiles;
using AuthService.Application.Abstractions.Security;
using AuthService.Infrastructure.Messaging;
using AuthService.Infrastructure.Observability;
using AuthService.Infrastructure.Outbox;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Profiles;
using AuthService.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopSystem.Contracts.Grpc.UserProfiles;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not found.");

        services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString));

        services.Configure<JwtRsaOptions>(configuration.GetSection(JwtRsaOptions.SectionName));
        services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));
        services.Configure<MessageBrokersOptions>(configuration.GetSection(MessageBrokersOptions.SectionName));
        services.Configure<AuthOutboxOptions>(configuration.GetSection(AuthOutboxOptions.SectionName));
        services.Configure<UserServiceClientOptions>(configuration.GetSection(UserServiceClientOptions.SectionName));

        services.AddScoped<IAuthUserRepository, AuthUserRepository>();
        services.AddScoped<IAuthOutboxService, AuthOutboxService>();
        services.AddScoped<IAuthOutboxBrokerPublisher, AuthOutboxBrokerPublisher>();
        services.AddSingleton<IPasswordHasherService, Pbkdf2PasswordHasherService>();
        services.AddSingleton<IJwtTokenService, RsaJwtTokenService>();
        services.AddHostedService<AuthOutboxWorker>();

        var userServiceGrpcAddress = configuration[$"{UserServiceClientOptions.SectionName}:GrpcAddress"]
            ?? configuration[$"{UserServiceClientOptions.SectionName}:BaseUrl"]
            ?? "http://localhost:5101";

        services.AddGrpcClient<UserProfilesGrpc.UserProfilesGrpcClient>(options =>
        {
            options.Address = new Uri(userServiceGrpcAddress);
        });

        services.AddScoped<IUserProfileProvisioningClient, UserProfileProvisioningClient>();

        var loggerServiceUrl = configuration[$"{LoggerServiceClientOptions.SectionName}:BaseUrl"] ?? "http://localhost:5300";
        services.AddHttpClient<ILoggerServiceClient, HttpLoggerServiceClient>(client =>
        {
            client.BaseAddress = new Uri(loggerServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
