using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using AuthenticationService.Api;
using AuthenticationService.Application.Abstractions.CQRS;
using AuthenticationService.Application.Features.Authentication.Commands.Login;
using AuthenticationService.Application.Features.Authentication.Commands.Register;
using AuthenticationService.Contracts.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AuthService.Api.IntegrationTests;

public sealed class RegisterEndpointTests
{
    [Fact]
    public async Task Register_WhenHandlerReturnsAuthResponse_Returns200()
    {
        await using var app = new TestAppFactory(_ => Task.FromResult<AuthResponseDto?>(
            new AuthResponseDto("token", DateTime.UtcNow.AddMinutes(30), Guid.NewGuid(), "ok@example.com", ["User"])));

        var client = app.CreateClient();

        var response = await client.PostAsJsonAsync("/api/authentication/register", new
        {
            Email = "ok@example.com",
            Password = "Secret123!",
            Roles = new[] { "User" }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhenHandlerReturnsNull_Returns409()
    {
        await using var app = new TestAppFactory(_ => Task.FromResult<AuthResponseDto?>(null));

        var client = app.CreateClient();

        var response = await client.PostAsJsonAsync("/api/authentication/register", new
        {
            Email = "dup@example.com",
            Password = "Secret123!",
            Roles = new[] { "User" }
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhenHandlerThrowsProfileProvisioningException_Returns503()
    {
        await using var app = new TestAppFactory(_ => throw new AuthenticationService.Application.Common.ProfileProvisioningException("temporary downstream issue", false));

        var client = app.CreateClient();

        var response = await client.PostAsJsonAsync("/api/authentication/register", new
        {
            Email = "retry@example.com",
            Password = "Secret123!",
            Roles = new[] { "User" }
        });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    private sealed class TestAppFactory(
        Func<RegisterCommand, Task<AuthResponseDto?>> registerBehavior) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            var rsa = RSA.Create(2048);
            var publicKey = rsa.ToXmlString(false);
            var privateKey = rsa.ToXmlString(true);

            builder.UseSetting("ConnectionStrings:PostgreSql", "Host=unused;Port=5432;Database=test;Username=test;Password=test");
            builder.UseSetting("JwtRsa:Issuer", "ShopSystem.Auth.Tests");
            builder.UseSetting("JwtRsa:Audience", "ShopSystem.Services.Tests");
            builder.UseSetting("JwtRsa:TokenLifetimeMinutes", "60");
            builder.UseSetting("JwtRsa:PublicKeyXml", publicKey);
            builder.UseSetting("JwtRsa:PrivateKeyXml", privateKey);
            builder.UseSetting("AdminSeed:Email", "admin@tests.local");
            builder.UseSetting("AdminSeed:Password", "Admin123!");
            builder.UseSetting("UserService:BaseUrl", "http://localhost:5101");
            builder.UseSetting("UserService:InternalApiKey", "test-key");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IHostedService>();

                services.RemoveAll(typeof(ICommandHandler<RegisterCommand, AuthResponseDto?>));
                services.RemoveAll(typeof(ICommandHandler<LoginCommand, AuthResponseDto?>));

                services.AddSingleton<ICommandHandler<RegisterCommand, AuthResponseDto?>>(new StubRegisterHandler(registerBehavior));
                services.AddSingleton<ICommandHandler<LoginCommand, AuthResponseDto?>>(new StubLoginHandler());
            });
        }
    }

    private sealed class StubRegisterHandler(
        Func<RegisterCommand, Task<AuthResponseDto?>> behavior) : ICommandHandler<RegisterCommand, AuthResponseDto?>
    {
        public Task<AuthResponseDto?> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            return behavior(command);
        }
    }

    private sealed class StubLoginHandler : ICommandHandler<LoginCommand, AuthResponseDto?>
    {
        public Task<AuthResponseDto?> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult<AuthResponseDto?>(null);
        }
    }
}
