using Edificia.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Edificia.Infrastructure.Identity;

/// <summary>
/// Adapts JwtSettings to IRefreshTokenSettings for the Application layer.
/// </summary>
public sealed class RefreshTokenSettingsAdapter : IRefreshTokenSettings
{
    public int ExpirationDays { get; }

    public RefreshTokenSettingsAdapter(IOptions<JwtSettings> jwtSettings)
    {
        ExpirationDays = jwtSettings.Value.RefreshTokenExpirationDays;
    }
}
