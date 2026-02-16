using Edificia.Application.Auth.Commands.Login;
using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Auth.Commands.RevokeToken;

/// <summary>
/// Handles refresh token revocation (logout).
/// Finds the token and marks it as revoked.
/// </summary>
public sealed class RevokeTokenHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<RevokeTokenHandler> _logger;

    public RevokeTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<RevokeTokenHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(
            request.Token, cancellationToken);

        if (token is null)
        {
            _logger.LogWarning("Revoke attempt for non-existent refresh token");
            return Result.Failure(AuthErrors.InvalidRefreshToken);
        }

        if (token.RevokedAt is not null)
        {
            // Already revoked — idempotent
            return Result.Success();
        }

        token.Revoke();
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token revoked for user {UserId}", token.UserId);

        return Result.Success();
    }
}
