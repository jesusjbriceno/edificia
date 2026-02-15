using Edificia.Application.Interfaces;
using Edificia.Infrastructure.Ai;
using Edificia.Infrastructure.Persistence;
using Edificia.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Edificia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ---------- EF Core (Write-side) ----------
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found in configuration.");

        services.AddDbContext<EdificiaDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        // ---------- Dapper (Read-side) ----------
        services.AddSingleton<IDbConnectionFactory, DapperConnectionFactory>();

        // ---------- Repositories ----------
        services.AddScoped<IProjectRepository, ProjectRepository>();

        // ---------- AI Service (Flux Gateway) ----------
        services.Configure<FluxGatewaySettings>(
            configuration.GetSection(FluxGatewaySettings.SectionName));

        services.AddMemoryCache();

        services.AddHttpClient<IAiService, FluxAiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
