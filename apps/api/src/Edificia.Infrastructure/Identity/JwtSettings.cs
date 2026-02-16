namespace Edificia.Infrastructure.Identity;

/// <summary>
/// JWT configuration settings.
/// Mapped from "Jwt" configuration section.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Secret key for signing tokens (min 32 chars).</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Token issuer (typically the API domain).</summary>
    public string Issuer { get; set; } = "https://api-edificia.jesusjbriceno.dev";

    /// <summary>Token audience (typically the web app domain).</summary>
    public string Audience { get; set; } = "https://edificia.jesusjbriceno.dev";

    /// <summary>Token expiration in minutes.</summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>Refresh token expiration in days.</summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
