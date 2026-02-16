using Edificia.Application.Auth.Commands.Login;
using Edificia.Application.Auth.DTOs;
using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Handles refresh token rotation:
/// 1. Validates the incoming refresh token.
/// 2. If the token was already revoked (reuse detection), revokes the entire family.
/// 3. Rotates: revokes old token, creates new refresh token, generates new access token.
/// 4. Returns updated LoginResponse with new tokens.
/// </summary>
public sealed class RefreshTokenHandler
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenSettings _refreshTokenSettings;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenSettings refreshTokenSettings,
        ILogger<RefreshTokenHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenSettings = refreshTokenSettings;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the refresh token
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(
            request.Token, cancellationToken);

        if (existingToken is null)
        {
            _logger.LogWarning("Refresh attempt with non-existent token");
            return Result.Failure<LoginResponse>(AuthErrors.InvalidRefreshToken);
        }

        // 2. Stolen token detection: if already revoked, revoke entire family
        if (existingToken.RevokedAt is not null)
        {
            _logger.LogWarning(
                "Reuse of revoked refresh token detected for user {UserId}. Revoking all tokens.",
                existingToken.UserId);

            await _refreshTokenRepository.RevokeAllForUserAsync(
                existingToken.UserId, cancellationToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            return Result.Failure<LoginResponse>(AuthErrors.InvalidRefreshToken);
        }

        // 3. Check expiration
        if (existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh attempt with expired token for user {UserId}",
                existingToken.UserId);
            return Result.Failure<LoginResponse>(AuthErrors.RefreshTokenExpired);
        }

        // 4. Find the associated user
        var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());

        if (user is null || !user.IsActive)
        {
            _logger.LogWarning("Refresh attempt for inactive/deleted user {UserId}",
                existingToken.UserId);
            return Result.Failure<LoginResponse>(AuthErrors.AccountInactive);
        }

        // 5. Rotate: create new refresh token
        var newRefreshToken = new Domain.Entities.RefreshToken(
            user.Id, _refreshTokenSettings.ExpirationDays);

        // 6. Revoke old token and link to new
        existingToken.Revoke(replacedByTokenId: newRefreshToken.Id);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        // 7. Generate new access token
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);

        // 8. Build response
        var response = new LoginResponse(
            AccessToken: accessToken,
            RefreshToken: newRefreshToken.Token,
            ExpiresInMinutes: 60,
            MustChangePassword: user.MustChangePassword,
            User: new UserInfo(
                Id: user.Id,
                Email: user.Email!,
                FullName: user.FullName,
                CollegiateNumber: user.CollegiateNumber,
                Roles: roles.ToList().AsReadOnly()));

        _logger.LogInformation("Token refreshed for user {UserId}", user.Id);

        return Result.Success(response);
    }
}
