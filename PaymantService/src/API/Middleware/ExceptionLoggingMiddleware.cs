using PaymantService.Infrastructure.Observability;

namespace PaymantService.Api.Middleware;

public sealed class ExceptionLoggingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ILoggerServiceClient loggerServiceClient)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString();
            logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId} Path={Path}",
                correlationId, context.Request.Path);

            _ = loggerServiceClient.SendAsync(
                "PaymantService",
                ex.ToString(),
                correlationId,
                CancellationToken.None);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(
                    new { error = "An unexpected error occurred.", correlationId });
            }
        }
    }
}
