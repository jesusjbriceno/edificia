using Edificia.Application.Interfaces;
using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Users.Commands.ResetUserPassword;

/// <summary>
/// Handles password reset by admin. Generates a temporary password,
/// sets MustChangePassword flag, and sends notification email.
/// Enforces role hierarchy: Admin cannot reset Root/Admin passwords.
/// </summary>
public sealed class ResetUserPasswordHandler : IRequestHandler<ResetUserPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<ResetUserPasswordHandler> _logger;

    public ResetUserPasswordHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<ResetUserPasswordHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the target user
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            _logger.LogWarning("Password reset attempt for non-existent user: {UserId}", request.UserId);
            return Result.Failure(UserErrors.UserNotFound);
        }

        // 2. Check role hierarchy
        var targetRoles = await _userManager.GetRolesAsync(user);
        var performerRoles = await GetUserRolesAsync(request.PerformedByUserId);

        if (!performerRoles.Contains(AppRoles.Root))
        {
            if (targetRoles.Contains(AppRoles.Root) || targetRoles.Contains(AppRoles.Admin))
            {
                _logger.LogWarning(
                    "User {PerformerId} attempted to reset password of higher-role user {UserId}",
                    request.PerformedByUserId, request.UserId);
                return Result.Failure(UserErrors.CannotModifyHigherRole);
            }
        }

        // 3. Generate temporary password
        var temporaryPassword = GenerateTemporaryPassword();

        // 4. Reset password using Identity
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, temporaryPassword);

        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to reset password for user {UserId}: {Errors}", request.UserId, errors);
            return Result.Failure(UserErrors.PasswordResetFailed);
        }

        // 5. Set MustChangePassword flag
        user.MustChangePassword = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // 6. Send reset email (fire-and-forget — don't block the HTTP response)
        var emailTo = user.Email!;
        var fullName = user.FullName;
        var tempPwd = temporaryPassword;
        _ = Task.Run(async () =>
        {
            try
            {
                var subject = "EDIFICIA - Su contraseña ha sido restablecida";
                var body = BuildResetEmail(fullName, emailTo, tempPwd);
                await _emailService.SendAsync(emailTo, subject, body, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Reset email could not be sent to {Email}. Password was reset successfully.", emailTo);
            }
        });

        _logger.LogInformation(
            "Password reset for user {UserId} by {PerformerId}", request.UserId, request.PerformedByUserId);

        return Result.Success();
    }

    private async Task<IList<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null
            ? await _userManager.GetRolesAsync(user)
            : Array.Empty<string>();
    }

    private static string GenerateTemporaryPassword()
    {
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%&*";

        var random = Random.Shared;
        var password = new char[12];

        password[0] = upper[random.Next(upper.Length)];
        password[1] = lower[random.Next(lower.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        var allChars = upper + lower + digits + special;
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        random.Shuffle(password);

        return new string(password);
    }

    private static string BuildResetEmail(string fullName, string email, string temporaryPassword)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; color: #333;">
                <h2>Contraseña restablecida - EDIFICIA</h2>
                <p>Hola <strong>{fullName}</strong>,</p>
                <p>Su contraseña ha sido restablecida por un administrador.</p>
                <p>A continuación encontrará sus nuevas credenciales:</p>
                <ul>
                    <li><strong>Email:</strong> {email}</li>
                    <li><strong>Nueva contraseña temporal:</strong> {temporaryPassword}</li>
                </ul>
                <p><strong>Importante:</strong> Deberá cambiar su contraseña en el próximo inicio de sesión.</p>
                <p>Acceda a la plataforma en: <a href="https://edificia.jesusjbriceno.dev">https://edificia.jesusjbriceno.dev</a></p>
                <br/>
                <p>Saludos,<br/>El equipo de EDIFICIA</p>
            </body>
            </html>
            """;
    }
}
