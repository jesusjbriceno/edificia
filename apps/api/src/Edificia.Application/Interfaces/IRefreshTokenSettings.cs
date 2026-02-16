namespace Edificia.Application.Interfaces;

/// <summary>
/// Provides refresh token configuration values.
/// Implemented by infrastructure layer reading from JwtSettings.
/// </summary>
public interface IRefreshTokenSettings
{
    /// <summary>Number of days until a refresh token expires.</summary>
    int ExpirationDays { get; }
}
