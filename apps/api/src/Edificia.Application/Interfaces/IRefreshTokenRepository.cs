using Edificia.Domain.Entities;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Repository for refresh token operations (write-side).
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>Finds an active (non-revoked, non-expired) token by its string value.</summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Adds a new refresh token.</summary>
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);

    /// <summary>Revokes all active tokens for a user (family revocation on stolen token detection).</summary>
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Persists pending changes.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
