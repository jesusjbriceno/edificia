using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Users.Commands.DeleteUser;

/// <summary>
/// Handles user deletion.
/// Enforces: cannot delete self, Admin cannot delete Root/Admin users.
/// </summary>
public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<DeleteUserHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Cannot delete own account
        if (request.UserId == request.PerformedByUserId)
        {
            _logger.LogWarning("User {UserId} attempted to delete their own account", request.UserId);
            return Result.Failure(UserErrors.CannotDeleteSelf);
        }

        // 2. Find the target user
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            _logger.LogWarning("Delete attempt for non-existent user: {UserId}", request.UserId);
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
                    "User {PerformerId} attempted to delete higher-role user {UserId}",
                    request.PerformedByUserId, request.UserId);
                return Result.Failure(UserErrors.CannotModifyHigherRole);
            }
        }

        // 4. Delete the user
        var deleteResult = await _userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
        {
            var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to delete user {UserId}: {Errors}", request.UserId, errors);
            return Result.Failure(UserErrors.DeleteFailed);
        }

        _logger.LogInformation(
            "User {UserId} deleted by {PerformerId}", request.UserId, request.PerformedByUserId);

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
