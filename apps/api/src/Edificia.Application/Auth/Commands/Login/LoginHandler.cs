using Edificia.Application.Auth.DTOs;
using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Auth.Commands.Login;

/// <summary>
/// Handles user authentication: validates credentials, checks account status,
/// generates JWT with MustChangePassword claim when required.
/// </summary>
public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenSettings _refreshTokenSettings;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenSettings refreshTokenSettings,
        ILogger<LoginHandler> logger)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenSettings = refreshTokenSettings;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            _logger.LogWarning("Login attempt for non-existent email: {Email}", request.Email);
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        // 2. Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {UserId}", user.Id);
            return Result.Failure<LoginResponse>(AuthErrors.AccountInactive);
        }

        // 3. Check if account is locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning("Login attempt for locked-out user: {UserId}", user.Id);
            return Result.Failure<LoginResponse>(AuthErrors.AccountLockedOut);
        }

        // 4. Validate password using UserManager (no SignInManager needed)
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
        {
            // Increment failed access count for lockout
            if (_userManager.SupportsUserLockout)
            {
                await _userManager.AccessFailedAsync(user);
            }

            _logger.LogWarning("Invalid password for user: {UserId}", user.Id);
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        // 5. Reset lockout count on successful login
        if (_userManager.SupportsUserLockout)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        // 6. Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // 7. Generate JWT (includes MustChangePassword claim if needed)
        var token = _jwtTokenService.GenerateAccessToken(user, roles);

        // 8. Create refresh token
        var refreshToken = new Domain.Entities.RefreshToken(
            user.Id, _refreshTokenSettings.ExpirationDays);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        // 9. Build response
        var response = new LoginResponse(
            AccessToken: token,
            RefreshToken: refreshToken.Token,
            ExpiresInMinutes: 60,
            MustChangePassword: user.MustChangePassword,
            User: new UserInfo(
                Id: user.Id,
                Email: user.Email!,
                FullName: user.FullName,
                CollegiateNumber: user.CollegiateNumber,
                Roles: roles.ToList().AsReadOnly()));

        _logger.LogInformation(
            "User {UserId} logged in successfully. MustChangePassword: {MustChange}",
            user.Id, user.MustChangePassword);

        return Result.Success(response);
    }
}

/// <summary>
/// Domain errors for authentication operations.
/// </summary>
public static class AuthErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "Email o contraseña incorrectos.");

    public static readonly Error AccountInactive =
        Error.Forbidden("Auth.AccountInactive", "La cuenta está desactivada. Contacte al administrador.");

    public static readonly Error AccountLockedOut =
        Error.Forbidden("Auth.AccountLockedOut", "La cuenta está bloqueada temporalmente por múltiples intentos fallidos.");

    public static readonly Error PasswordChangeFailed =
        Error.Failure("Auth.PasswordChangeFailed", "No se pudo cambiar la contraseña.");

    public static readonly Error InvalidCurrentPassword =
        Error.Unauthorized("Auth.InvalidCurrentPassword", "La contraseña actual es incorrecta.");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized("Auth.InvalidRefreshToken", "El token de actualización no es válido.");

    public static readonly Error RefreshTokenExpired =
        Error.Unauthorized("Auth.RefreshTokenExpired", "El token de actualización ha expirado.");

    public static readonly Error ProfileUpdateFailed =
        Error.Failure("Auth.ProfileUpdateFailed", "No se pudo actualizar el perfil.");
}
