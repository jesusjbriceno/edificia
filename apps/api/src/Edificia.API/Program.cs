using Edificia.API.Configuration;
using Edificia.API.Middleware;
using Edificia.Application;
using Edificia.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// ---------- Middleware Pipeline ----------

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

app.Run();
