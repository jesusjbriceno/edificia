using System.Security.Cryptography;
using Edificia.Domain.Primitives;

namespace Edificia.Domain.Entities;

/// <summary>
/// Represents a refresh token issued alongside a JWT access token.
/// Supports token rotation: each refresh invalidates the previous token
/// and issues a new one, linking via ReplacedByTokenId.
/// </summary>
public sealed class RefreshToken : Entity
{
    /// <summary>FK → ApplicationUser.</summary>
    public Guid UserId { get; set; }

    /// <summary>Opaque token string (64 bytes, Base64-encoded).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Absolute expiration date (UTC).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Creation timestamp (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>If set, the token was revoked at this time.</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Points to the replacement token created during rotation.
    /// Used for stolen-token detection (if a revoked token is reused,
    /// the entire family chain is revoked).
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    /// <summary>Indicates whether the token is still valid.</summary>
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;

    /// <summary>Navigation property → ApplicationUser.</summary>
    public ApplicationUser? User { get; set; }

    // EF Core requires parameterless constructor
    private RefreshToken() { }

    public RefreshToken(Guid userId, int expirationDays)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        Token = GenerateToken();
        ExpiresAt = DateTime.UtcNow.AddDays(expirationDays);
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes this token and records which token replaced it.
    /// </summary>
    public void Revoke(Guid? replacedByTokenId = null)
    {
        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }

    /// <summary>
    /// Generates a cryptographically secure random token (64 bytes → Base64).
    /// </summary>
    private static string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
