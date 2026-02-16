using Edificia.Application.Auth.Commands.Login;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Auth.Commands.ChangePassword;

/// <summary>
/// Handles password change. If the user had MustChangePassword = true,
/// clears that flag so subsequent JWTs grant full access.
/// </summary>
public sealed class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ChangePasswordHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by ID (from JWT claim)
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            _logger.LogWarning("Password change attempt for non-existent user: {UserId}", request.UserId);
            return Result.Failure(AuthErrors.InvalidCredentials);
        }

        // 2. Change password (Identity validates the current password)
        var changeResult = await _userManager.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);

        if (!changeResult.Succeeded)
        {
            var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Password change failed for user {UserId}: {Errors}", request.UserId, errors);

            // Check if it's a wrong current password
            if (changeResult.Errors.Any(e => e.Code == "PasswordMismatch"))
                return Result.Failure(AuthErrors.InvalidCurrentPassword);

            return Result.Failure(AuthErrors.PasswordChangeFailed);
        }

        // 3. Clear MustChangePassword flag if it was set
        if (user.MustChangePassword)
        {
            user.MustChangePassword = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation(
                "User {UserId} completed mandatory password change.", request.UserId);
        }
        else
        {
            _logger.LogInformation(
                "User {UserId} changed password voluntarily.", request.UserId);
        }

        return Result.Success();
    }
}
