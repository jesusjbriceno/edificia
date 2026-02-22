using System.Text;
using Edificia.Application.Interfaces;
using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using Edificia.Infrastructure.Ai;
using Edificia.Infrastructure.Email;
using Edificia.Infrastructure.Export;
using Edificia.Infrastructure.Identity;
using Edificia.Infrastructure.Persistence;
using Edificia.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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

        // ---------- ASP.NET Core Identity ----------
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                // Password policy
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Lockout
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // User
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<EdificiaDbContext>()
            .AddDefaultTokenProviders();

        // ---------- Identity Seeder ----------
        services.Configure<SecuritySettings>(
            configuration.GetSection(SecuritySettings.SectionName));

        services.AddHostedService<IdentityDataInitializer>();

        // ---------- JWT Settings ----------
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        // ---------- JWT Token Service ----------
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IRefreshTokenSettings, RefreshTokenSettingsAdapter>();

        // ---------- JWT Bearer Authentication ----------
        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>() ?? new JwtSettings();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        // ---------- Authorization Policies ----------
        services.AddAuthorizationBuilder()
            .AddPolicy(AppPolicies.ActiveUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    !context.User.HasClaim(AppClaims.AuthMethodReference, AppClaims.PasswordChangeRequired));
            })
            .AddPolicy(AppPolicies.RequireRoot, policy =>
            {
                policy.RequireRole(AppRoles.Root);
            })
            .AddPolicy(AppPolicies.RequireAdmin, policy =>
            {
                policy.RequireRole(AppRoles.Root, AppRoles.Admin);
            })
            .AddPolicy(AppPolicies.RequireArchitect, policy =>
            {
                policy.RequireRole(AppRoles.Root, AppRoles.Admin, AppRoles.Architect);
            });

        // ---------- Email Service ----------
        services.Configure<EmailSettings>(
            configuration.GetSection(EmailSettings.SectionName));

        var emailSection = configuration.GetSection(EmailSettings.SectionName);
        var emailProvider = emailSection.GetValue<string>("Provider") ?? "Smtp";

        // Log at startup to aid debugging which provider is active
        Console.WriteLine($"[Edificia] Email provider configured: {emailProvider}");

        if (emailProvider.Equals("Brevo", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IEmailService, BrevoEmailService>(client =>
            {
                var apiKey = emailSection.GetValue<string>("BrevoApiKey") ?? string.Empty;

                client.BaseAddress = new Uri(
                    emailSection.GetValue<string>("BrevoApiUrl") ?? "https://api.brevo.com/v3");
                client.DefaultRequestHeaders.Add("api-key", apiKey);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        }
        else
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
        }

        // ---------- Dapper (Read-side) ----------
        services.AddSingleton<IDbConnectionFactory, DapperConnectionFactory>();

        // ---------- Repositories ----------
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // ---------- Application Services ----------
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUserQueryService, UserQueryService>();

        // ---------- Document Export ----------
        services.AddScoped<IDocumentExportService, DocxExportService>();

        // ---------- AI Service (n8n Webhook) ----------
        services.Configure<N8nAiSettings>(
            configuration.GetSection(N8nAiSettings.SectionName));

        var aiSettings = configuration
            .GetSection(N8nAiSettings.SectionName)
            .Get<N8nAiSettings>() ?? new N8nAiSettings();

        services.AddHttpClient<IAiService, N8nAiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(aiSettings.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
