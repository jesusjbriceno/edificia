namespace Edificia.API.Middleware;

/// <summary>
/// Extension methods for registering custom middleware.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds correlation ID middleware that generates/propagates X-Correlation-Id headers
    /// and enriches Serilog's LogContext.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
