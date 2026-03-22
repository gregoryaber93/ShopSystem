using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using PromotionService.Application;
using PromotionService.Infrastructure;
using PromotionService.Infrastructure.Persistence;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using PromotionService.Api.Grpc;
using PromotionService.Api.Middleware;
using PromotionService.Api.Security;

namespace PromotionService.Api;

public class Program
{
    private const string CorsPolicyName = "FrontendDev";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddGrpc();
        builder.Services.AddControllers();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Wpisz token JWT w formacie: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
        });

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter());

        var jwtRsaOptions = builder.Configuration.GetSection(JwtRsaOptions.SectionName).Get<JwtRsaOptions>()
            ?? throw new InvalidOperationException("JwtRsa section is missing in configuration.");

        if (string.IsNullOrWhiteSpace(jwtRsaOptions.PublicKeyXml))
        {
            throw new InvalidOperationException("JwtRsa:PublicKeyXml is required for JWT RSA validation.");
        }

        using var validationRsa = RSA.Create();
        validationRsa.FromXmlString(jwtRsaOptions.PublicKeyXml);
        var validationKey = new RsaSecurityKey(validationRsa.ExportParameters(false));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtRsaOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtRsaOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = validationKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = ClaimTypes.Role
                };
            });

        builder.Services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PromotionDbContext>();
            dbContext.Database.EnsureCreated();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors(CorsPolicyName);
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ExceptionLoggingMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGrpcService<PromotionsGrpcService>();
        app.MapControllers();

        app.Run();
    }
}
