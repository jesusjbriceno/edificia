using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Edificia.Infrastructure.Identity;

/// <summary>
/// Hosted service that seeds identity data (roles + Root user) on application startup.
/// Runs once at startup and then completes.
/// </summary>
public sealed class IdentityDataInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SecuritySettings _securitySettings;
    private readonly ILogger<IdentityDataInitializer> _logger;

    public IdentityDataInitializer(
        IServiceProvider serviceProvider,
        IOptions<SecuritySettings> securitySettings,
        ILogger<IdentityDataInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _securitySettings = securitySettings.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager);
        await SeedRootUserAsync(userManager);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var roleName in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));

                if (result.Succeeded)
                    _logger.LogInformation("Role '{Role}' created successfully", roleName);
                else
                    _logger.LogError("Failed to create role '{Role}': {Errors}",
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedRootUserAsync(UserManager<ApplicationUser> userManager)
    {
        // Check if any Root user already exists
        var existingRootUsers = await userManager.GetUsersInRoleAsync(AppRoles.Root);
        if (existingRootUsers.Count > 0)
        {
            _logger.LogInformation("Root user already exists, skipping seed");
            return;
        }

        var rootUser = new ApplicationUser
        {
            UserName = _securitySettings.RootEmail,
            Email = _securitySettings.RootEmail,
            EmailConfirmed = true,
            FullName = "Administrador del Sistema",
            MustChangePassword = true,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(rootUser, _securitySettings.RootInitialPassword);

        if (!createResult.Succeeded)
        {
            _logger.LogCritical("Failed to create Root user: {Errors}",
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(rootUser, AppRoles.Root);

        if (roleResult.Succeeded)
        {
            _logger.LogInformation(
                "Root user created successfully with email '{Email}'. " +
                "IMPORTANT: Password must be changed on first login.",
                _securitySettings.RootEmail);
        }
        else
        {
            _logger.LogError("Failed to assign Root role: {Errors}",
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    }
}
