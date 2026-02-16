using Edificia.Application.Interfaces;
using Edificia.Domain.Constants;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Users.Commands.CreateUser;

/// <summary>
/// Handles user creation. Generates a temporary password, assigns role,
/// sets MustChangePassword flag, and sends welcome email.
/// Enforces role hierarchy: Admin cannot create Root/Admin users.
/// </summary>
public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<CreateUserHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate role hierarchy
        var creatorRoles = await GetUserRolesAsync(request.CreatedByUserId);
        if (!creatorRoles.Contains(AppRoles.Root) &&
            (request.Role == AppRoles.Root || request.Role == AppRoles.Admin))
        {
            _logger.LogWarning(
                "User {CreatorId} attempted to create a user with role {Role} without permission",
                request.CreatedByUserId, request.Role);
            return Result.Failure<Guid>(UserErrors.CannotModifyHigherRole);
        }

        // 2. Check email uniqueness
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            _logger.LogWarning("Attempt to create user with existing email: {Email}", request.Email);
            return Result.Failure<Guid>(UserErrors.EmailAlreadyExists);
        }

        // 3. Generate temporary password
        var temporaryPassword = GenerateTemporaryPassword();

        // 4. Create user entity
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            FullName = request.FullName,
            CollegiateNumber = request.CollegiateNumber,
            MustChangePassword = true,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, temporaryPassword);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user {Email}: {Errors}", request.Email, errors);
            return Result.Failure<Guid>(UserErrors.CreationFailed);
        }

        // 5. Assign role
        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);

        if (!roleResult.Succeeded)
        {
            _logger.LogError("Failed to assign role {Role} to user {UserId}", request.Role, user.Id);
            // Rollback: delete the user
            await _userManager.DeleteAsync(user);
            return Result.Failure<Guid>(UserErrors.RoleChangeFailed);
        }

        // 6. Send welcome email
        try
        {
            var subject = "Bienvenido a EDIFICIA - Su cuenta ha sido creada";
            var body = BuildWelcomeEmail(request.FullName, request.Email, temporaryPassword);
            await _emailService.SendAsync(request.Email, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log but don't fail the operation - user was created successfully
            _logger.LogWarning(ex,
                "Welcome email could not be sent to {Email}. User was created successfully.", request.Email);
        }

        _logger.LogInformation(
            "User {UserId} created with role {Role} by {CreatorId}",
            user.Id, request.Role, request.CreatedByUserId);

        return Result.Success(user.Id);
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
        // Generate a secure temporary password meeting all complexity requirements
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%&*";

        var random = Random.Shared;
        var password = new char[12];

        // Ensure at least one of each required type
        password[0] = upper[random.Next(upper.Length)];
        password[1] = lower[random.Next(lower.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        // Fill remaining with random mix
        var allChars = upper + lower + digits + special;
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        // Shuffle to avoid predictable positions
        random.Shuffle(password);

        return new string(password);
    }

    private static string BuildWelcomeEmail(string fullName, string email, string temporaryPassword)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; color: #333;">
                <h2>Bienvenido a EDIFICIA</h2>
                <p>Hola <strong>{fullName}</strong>,</p>
                <p>Su cuenta ha sido creada en la plataforma EDIFICIA.</p>
                <p>A continuación encontrará sus credenciales de acceso:</p>
                <ul>
                    <li><strong>Email:</strong> {email}</li>
                    <li><strong>Contraseña temporal:</strong> {temporaryPassword}</li>
                </ul>
                <p><strong>Importante:</strong> Deberá cambiar su contraseña en el primer inicio de sesión.</p>
                <p>Acceda a la plataforma en: <a href="https://edificia.jesusjbriceno.dev">https://edificia.jesusjbriceno.dev</a></p>
                <br/>
                <p>Saludos,<br/>El equipo de EDIFICIA</p>
            </body>
            </html>
            """;
    }
}
