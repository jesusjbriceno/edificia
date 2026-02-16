using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Users.Commands.UpdateUser;

/// <summary>
/// Handles user update. Updates profile fields and role.
/// Enforces role hierarchy: Admin cannot modify Root/Admin users.
/// </summary>
public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<UpdateUserHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the target user
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            _logger.LogWarning("Update attempt for non-existent user: {UserId}", request.UserId);
            return Result.Failure(UserErrors.UserNotFound);
        }

        // 2. Check role hierarchy
        var targetCurrentRoles = await _userManager.GetRolesAsync(user);
        var updaterRoles = await GetUserRolesAsync(request.UpdatedByUserId);

        if (!updaterRoles.Contains(AppRoles.Root))
        {
            // Admin cannot modify Root or other Admin users
            if (targetCurrentRoles.Contains(AppRoles.Root) || targetCurrentRoles.Contains(AppRoles.Admin))
            {
                _logger.LogWarning(
                    "User {UpdaterId} attempted to modify user {UserId} with higher/equal role",
                    request.UpdatedByUserId, request.UserId);
                return Result.Failure(UserErrors.CannotModifyHigherRole);
            }

            // Admin cannot assign Root or Admin role
            if (request.Role == AppRoles.Root || request.Role == AppRoles.Admin)
            {
                _logger.LogWarning(
                    "User {UpdaterId} attempted to assign role {Role} without permission",
                    request.UpdatedByUserId, request.Role);
                return Result.Failure(UserErrors.CannotModifyHigherRole);
            }
        }

        // 3. Update profile fields
        user.FullName = request.FullName;
        user.CollegiateNumber = request.CollegiateNumber;
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to update user {UserId}: {Errors}", request.UserId, errors);
            return Result.Failure(UserErrors.UpdateFailed);
        }

        // 4. Update role if changed
        var currentRole = targetCurrentRoles.FirstOrDefault();

        if (currentRole != request.Role)
        {
            if (currentRole is not null)
            {
                var removeResult = await _userManager.RemoveFromRoleAsync(user, currentRole);
                if (!removeResult.Succeeded)
                {
                    _logger.LogError("Failed to remove role {Role} from user {UserId}",
                        currentRole, request.UserId);
                    return Result.Failure(UserErrors.RoleChangeFailed);
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!addResult.Succeeded)
            {
                _logger.LogError("Failed to assign role {Role} to user {UserId}",
                    request.Role, request.UserId);
                // Try to re-add the original role
                if (currentRole is not null)
                    await _userManager.AddToRoleAsync(user, currentRole);
                return Result.Failure(UserErrors.RoleChangeFailed);
            }

            _logger.LogInformation(
                "Role changed for user {UserId}: {OldRole} -> {NewRole} by {UpdaterId}",
                request.UserId, currentRole, request.Role, request.UpdatedByUserId);
        }

        _logger.LogInformation(
            "User {UserId} updated by {UpdaterId}", request.UserId, request.UpdatedByUserId);

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
