using Edificia.API.Middleware;
using Edificia.Application;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------

// Global Exception Handler (RFC 7807 ProblemDetails)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Application Layer (MediatR + FluentValidation pipeline)
builder.Services.AddApplication();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ---------- Middleware Pipeline ----------

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
