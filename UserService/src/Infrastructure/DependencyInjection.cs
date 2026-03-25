using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Abstractions.Identity;
using UserService.Application.Abstractions.Persistence;
using UserService.Application.Abstractions.Security;
using UserService.Infrastructure.Identity;
using UserService.Infrastructure.Messaging;
using UserService.Infrastructure.Observability;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Security;
using ShopSystem.Contracts.Grpc.AuthIdentity;

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
        services.Configure<MessageBrokersOptions>(configuration.GetSection(MessageBrokersOptions.SectionName));
        services.Configure<InternalApiOptions>(configuration.GetSection(InternalApiOptions.SectionName));
        services.Configure<AuthenticationServiceClientOptions>(configuration.GetSection(AuthenticationServiceClientOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        services.AddHostedService<UserCreatedConsumerWorker>();

        var authenticationServiceGrpcAddress = configuration[$"{AuthenticationServiceClientOptions.SectionName}:GrpcAddress"]
            ?? configuration[$"{AuthenticationServiceClientOptions.SectionName}:BaseUrl"]
            ?? "http://localhost:5295";

        services.AddGrpcClient<AuthIdentityGrpc.AuthIdentityGrpcClient>(options =>
        {
            options.Address = new Uri(authenticationServiceGrpcAddress);
        });
        services.AddScoped<IAuthIdentityProvisioningClient, AuthIdentityProvisioningClient>();

        var loggerServiceUrl = configuration[$"{LoggerServiceClientOptions.SectionName}:BaseUrl"] ?? "http://localhost:5300";
        services.AddHttpClient<ILoggerServiceClient, HttpLoggerServiceClient>(client =>
        {
            client.BaseAddress = new Uri(loggerServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
