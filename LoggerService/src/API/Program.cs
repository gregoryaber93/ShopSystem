using LoggerService.Application;
using LoggerService.Api.Middleware;
using LoggerService.Infrastructure;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace LoggerService.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter());

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<LoggerService.Infrastructure.Persistence.LoggerDbContext>();
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

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ExceptionLoggingMiddleware>();


        app.MapControllers();
        app.Run();
    }
}
