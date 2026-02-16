using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Users.Commands.ToggleUserStatus;

/// <summary>
/// Handles user activation/deactivation.
/// Enforces: cannot deactivate self, Admin cannot toggle Root/Admin users.
/// </summary>
public sealed class ToggleUserStatusHandler : IRequestHandler<ToggleUserStatusCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ToggleUserStatusHandler> _logger;

    public ToggleUserStatusHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ToggleUserStatusHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        // 1. Cannot toggle own account
        if (request.UserId == request.PerformedByUserId)
        {
            _logger.LogWarning("User {UserId} attempted to toggle their own status", request.UserId);
            return Result.Failure(UserErrors.CannotDeactivateSelf);
        }

        // 2. Find the target user
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            _logger.LogWarning("Toggle status attempt for non-existent user: {UserId}", request.UserId);
            return Result.Failure(UserErrors.UserNotFound);
        }

        // 3. Check role hierarchy
        var targetRoles = await _userManager.GetRolesAsync(user);
        var performerRoles = await GetUserRolesAsync(request.PerformedByUserId);

        if (!performerRoles.Contains(AppRoles.Root))
        {
            if (targetRoles.Contains(AppRoles.Root) || targetRoles.Contains(AppRoles.Admin))
            {
                _logger.LogWarning(
                    "User {PerformerId} attempted to toggle status of higher-role user {UserId}",
                    request.PerformedByUserId, request.UserId);
                return Result.Failure(UserErrors.CannotModifyHigherRole);
            }
        }

        // 4. Toggle status
        user.IsActive = request.Activate;
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to toggle status for user {UserId}: {Errors}", request.UserId, errors);
            return Result.Failure(UserErrors.UpdateFailed);
        }

        var action = request.Activate ? "activated" : "deactivated";
        _logger.LogInformation(
            "User {UserId} {Action} by {PerformerId}", request.UserId, action, request.PerformedByUserId);

        return Result.Success();
    }

    private async Task<IList<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null
            ? await _userManager.GetRolesAsync(user)
            : Array.Empty<string>();
    }
}
