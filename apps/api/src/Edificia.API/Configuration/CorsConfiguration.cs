namespace Edificia.API.Configuration;

/// <summary>
/// CORS policy configuration.
/// Reads allowed origins from configuration (Cors:AllowedOrigins).
/// </summary>
public static class CorsConfiguration
{
    public const string PolicyName = "EdificiaCors";

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
                else
                {
                    // Fallback: no origins configured, deny all cross-origin
                    policy
                        .SetIsOriginAllowed(_ => false)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        return services;
    }
}
