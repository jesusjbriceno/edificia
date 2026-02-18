using Edificia.API.Configuration;
using Edificia.API.Middleware;
using Edificia.Application;
using Edificia.Infrastructure;
using Serilog;

// ---------- Bootstrap Serilog ----------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    // Map friendly env var names (DB_HOST, JWT_SECRET…) → .NET config keys
    // BEFORE CreateBuilder so the environment variable provider picks them up.
    MapEnvironmentVariables();

    var builder = WebApplication.CreateBuilder(args);

    // ---------- Serilog ----------
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId:l} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("logs/edificia-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"));

    // ---------- Services ----------

    // Global Exception Handler (RFC 7807 ProblemDetails)
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Application Layer (MediatR + FluentValidation pipeline)
    builder.Services.AddApplication();

    // Infrastructure Layer (EF Core + Dapper)
    builder.Services.AddInfrastructure(builder.Configuration);

    // CORS
    builder.Services.AddCorsPolicy(builder.Configuration);

    builder.Services.AddControllers();

    // Swagger / OpenAPI
    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerDocumentation();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."),
            name: "postgresql",
            tags: ["db", "ready"]);

    var app = builder.Build();

    // ---------- Middleware Pipeline ----------

    app.UseCorrelationId();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault() ?? string.Empty);
        };
    });

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local")
    {
        app.MapOpenApi();
        app.UseSwaggerDocumentation();
    }

    app.UseCors(CorsConfiguration.PolicyName);

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false // Just checks if the app is running
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// ---------- Environment Variable Mapping ----------
// Translates short, human-friendly env var names (DB_HOST, JWT_SECRET, etc.)
// into the .NET configuration key convention (ConnectionStrings__DefaultConnection,
// Jwt__SecretKey, etc.).  This lets .env files and container platforms (Coolify,
// Portainer …) inject configuration without knowing .NET internals.
//
// Priority: if the target .NET key is already set (e.g. via
// ConnectionStrings__DefaultConnection), the short name is ignored.
static void MapEnvironmentVariables()
{
    // ── Connection string from individual DB_* vars ──
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    if (!string.IsNullOrEmpty(dbHost)
        && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")))
    {
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var name = Environment.GetEnvironmentVariable("DB_NAME") ?? "edificia_db";
        var user = Environment.GetEnvironmentVariable("DB_USER") ?? "edificia";
        var pass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            $"Host={dbHost};Port={port};Database={name};Username={user};Password={pass}");
    }

    // ── Short name → .NET config key ──
    MapEnv("JWT_SECRET",     "Jwt__SecretKey");
    MapEnv("N8N_WEBHOOK_URL", "AI__WebhookUrl");
    MapEnv("N8N_API_SECRET",  "AI__ApiSecret");
    MapEnv("N8N_TIMEOUT",     "AI__TimeoutSeconds");
    MapEnv("EMAIL_PROVIDER",  "Email__Provider");
    MapEnv("BREVO_API_KEY",   "Email__BrevoApiKey");
    MapEnv("SMTP_HOST",       "Email__SmtpHost");
    MapEnv("SMTP_PORT",       "Email__SmtpPort");
    MapEnv("SMTP_USERNAME",   "Email__SmtpUsername");
    MapEnv("SMTP_PASSWORD",   "Email__SmtpPassword");
    MapEnv("SMTP_USE_SSL",    "Email__SmtpUseSsl");
    MapEnv("ROOT_EMAIL",      "Security__RootEmail");
    MapEnv("ROOT_PASSWORD",   "Security__RootInitialPassword");

    static void MapEnv(string source, string target)
    {
        var value = Environment.GetEnvironmentVariable(source);
        if (!string.IsNullOrEmpty(value)
            && string.IsNullOrEmpty(Environment.GetEnvironmentVariable(target)))
        {
            Environment.SetEnvironmentVariable(target, value);
        }
    }
}
