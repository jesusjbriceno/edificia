using Edificia.API.Configuration;
using Edificia.API.Middleware;
using Edificia.Application;
using Edificia.Infrastructure;
using Edificia.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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

    // ---------- Apply Pending Migrations ----------
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<EdificiaDbContext>();
        dbContext.Database.Migrate();
    }

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
// Supports three DB connection patterns (checked in priority order):
//   1. ConnectionStrings__DefaultConnection already set → used as-is
//   2. DATABASE_URL in URI format (postgres://user:pass@host:port/db) → parsed
//   3. Individual DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD → assembled
static void MapEnvironmentVariables()
{
    const string connStringKey = "ConnectionStrings__DefaultConnection";

    // Skip if the .NET key is already explicitly provided
    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(connStringKey)))
    {
        // ── Option 1: DATABASE_URL in URI format ──
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            var parsed = ParseDatabaseUrl(databaseUrl);
            if (parsed is not null)
            {
                Environment.SetEnvironmentVariable(connStringKey, parsed);
                Log.Information("ConnectionString resolved from DATABASE_URL (host: {Host})",
                    ExtractHost(databaseUrl));
            }
            else
            {
                Log.Warning("DATABASE_URL is set but could not be parsed: {Url}",
                    MaskPassword(databaseUrl));
            }
        }
        else
        {
            // ── Option 2: Individual DB_* variables ──
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            if (!string.IsNullOrEmpty(dbHost))
            {
                var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
                var name = Environment.GetEnvironmentVariable("DB_NAME") ?? "edificia_db";
                var user = Environment.GetEnvironmentVariable("DB_USER") ?? "edificia";
                var pass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

                Environment.SetEnvironmentVariable(connStringKey,
                    $"Host={dbHost};Port={port};Database={name};Username={user};Password={pass}");

                Log.Information("ConnectionString assembled from DB_* vars (host: {Host}:{Port})",
                    dbHost, port);
            }
            else
            {
                Log.Warning("No database connection configured. Set DATABASE_URL, DB_HOST, " +
                            "or ConnectionStrings__DefaultConnection");
            }
        }
    }
    else
    {
        Log.Information("ConnectionString provided directly via ConnectionStrings__DefaultConnection");
    }

    // ── Short name → .NET config key ──
    MapEnv("JWT_SECRET",     "Jwt__SecretKey");
    MapEnv("N8N_WEBHOOK_URL", "AI__WebhookUrl");
    MapEnv("N8N_API_SECRET",  "AI__ApiSecret");
    MapEnv("N8N_TIMEOUT",     "AI__TimeoutSeconds");
    MapEnv("EMAIL_PROVIDER",     "Email__Provider");
    MapEnv("EMAIL_FROM_ADDRESS", "Email__FromAddress");
    MapEnv("EMAIL_FROM_NAME",    "Email__FromName");
    MapEnv("BREVO_API_KEY",      "Email__BrevoApiKey");
    MapEnv("SMTP_HOST",          "Email__SmtpHost");
    MapEnv("SMTP_PORT",          "Email__SmtpPort");
    MapEnv("SMTP_USERNAME",      "Email__SmtpUsername");
    MapEnv("SMTP_PASSWORD",      "Email__SmtpPassword");
    MapEnv("SMTP_USE_SSL",       "Email__SmtpUseSsl");
    MapEnv("ROOT_EMAIL",      "Security__RootEmail");
    MapEnv("ROOT_PASSWORD",   "Security__RootInitialPassword");

    // ── Helpers ──

    static void MapEnv(string source, string target)
    {
        var value = Environment.GetEnvironmentVariable(source);
        if (!string.IsNullOrEmpty(value)
            && string.IsNullOrEmpty(Environment.GetEnvironmentVariable(target)))
        {
            Environment.SetEnvironmentVariable(target, value);
        }
    }

    // Parses postgres://user:pass@host:port/dbname → Npgsql keyword format
    // Accepts both "postgres://" and "postgresql://" schemes.
    static string? ParseDatabaseUrl(string url)
    {
        // Normalize scheme so Uri class can parse it
        if (url.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
            url = "postgresql" + url[8..]; // replace "postgres" with "postgresql"

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return null;

        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? "");
        var password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? "");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(database))
            return null;

        // Preserve any query-string params (e.g. ?sslmode=require) as extra Npgsql keys
        var extra = "";
        if (!string.IsNullOrEmpty(uri.Query))
        {
            var queryParams = uri.Query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(p =>
                {
                    var kv = p.Split('=', 2);
                    return kv.Length == 2
                        ? $"{Uri.UnescapeDataString(kv[0])}={Uri.UnescapeDataString(kv[1])}"
                        : null;
                })
                .Where(p => p is not null);
            extra = ";" + string.Join(";", queryParams);
        }

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}{extra}";
    }

    static string ExtractHost(string url)
    {
        try { return new Uri(url.Replace("postgres://", "postgresql://")).Host; }
        catch { return "(unknown)"; }
    }

    static string MaskPassword(string url)
    {
        try
        {
            var uri = new Uri(url.Replace("postgres://", "postgresql://"));
            return uri.UserInfo.Contains(':')
                ? url.Replace(uri.UserInfo, uri.UserInfo.Split(':')[0] + ":****")
                : url;
        }
        catch { return "(unparseable)"; }
    }
}
