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
