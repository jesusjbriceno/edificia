using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Handles the public forgot-password flow.
/// Generates a temporary password, sets MustChangePassword = true,
/// and sends the credentials via email (fire-and-forget).
/// Always returns success to prevent email enumeration attacks.
/// </summary>
public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    public ForgotPasswordHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<ForgotPasswordHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Always return success to prevent email enumeration
        if (user is null || !user.IsActive)
        {
            _logger.LogInformation(
                "Forgot-password requested for unknown/inactive email: {Email}", request.Email);
            return Result.Success();
        }

        // Generate temporary password
        var temporaryPassword = GenerateTemporaryPassword();

        // Reset password using Identity
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, temporaryPassword);

        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            _logger.LogError(
                "Forgot-password: failed to reset password for user {UserId}: {Errors}",
                user.Id, errors);
            // Still return success to prevent enumeration
            return Result.Success();
        }

        // Set MustChangePassword flag
        user.MustChangePassword = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Send email (fire-and-forget)
        var emailTo = user.Email!;
        var fullName = user.FullName;
        var tempPwd = temporaryPassword;
        _ = Task.Run(async () =>
        {
            try
            {
                var subject = "EDIFICIA - Recuperación de contraseña";
                var body = BuildRecoveryEmail(fullName, emailTo, tempPwd);
                await _emailService.SendAsync(emailTo, subject, body, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Recovery email could not be sent to {Email}.", emailTo);
            }
        });

        _logger.LogInformation("Forgot-password requested for user {UserId}", user.Id);

        return Result.Success();
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
            password[i] = allChars[random.Next(allChars.Length)];

        random.Shuffle(password);
        return new string(password);
    }

    private static string BuildRecoveryEmail(string fullName, string email, string temporaryPassword)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; color: #333;">
                <h2>Recuperación de contraseña - EDIFICIA</h2>
                <p>Hola <strong>{fullName}</strong>,</p>
                <p>Ha solicitado restablecer su contraseña en la plataforma EDIFICIA.</p>
                <p>A continuación encontrará sus nuevas credenciales de acceso:</p>
                <ul>
                    <li><strong>Email:</strong> {email}</li>
                    <li><strong>Nueva contraseña temporal:</strong> {temporaryPassword}</li>
                </ul>
                <p><strong>Importante:</strong> Deberá cambiar su contraseña en el próximo inicio de sesión.</p>
                <p>Si usted no solicitó este cambio, contacte al administrador de la plataforma.</p>
                <p>Acceda a la plataforma en: <a href="https://edificia.jesusjbriceno.dev">https://edificia.jesusjbriceno.dev</a></p>
                <br/>
                <p>Saludos,<br/>El equipo de EDIFICIA</p>
            </body>
            </html>
            """;
    }
}
