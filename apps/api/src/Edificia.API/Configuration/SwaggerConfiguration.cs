using Microsoft.OpenApi;

namespace Edificia.API.Configuration;

/// <summary>
/// Swagger/OpenAPI configuration with JWT Bearer support.
/// Centralizes all Swagger settings for maintainability.
/// </summary>
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EDIFICIA API",
                Version = "v1",
                Description = "API para la redacción automatizada de Memorias de Proyecto de Ejecución (CTE/LOE).",
                Contact = new OpenApiContact
                {
                    Name = "EDIFICIA",
                    Url = new Uri("https://edificia.jesusjbriceno.dev")
                },
                License = new OpenApiLicense
                {
                    Name = "Apache 2.0",
                    Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0")
                }
            });

            // JWT Bearer scheme — ready for future authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Description = "Introduce el token JWT. Ejemplo: eyJhbGciOiJIUzI1NiIs..."
            });

            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    new List<string>()
                }
            });

            // Include XML comments if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "EDIFICIA API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "EDIFICIA API - Swagger";
        });

        return app;
    }
}
