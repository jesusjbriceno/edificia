using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Infrastructure.Identity;

/// <summary>
/// EF Core implementation of IRefreshTokenRepository.
/// Handles CRUD and family revocation for refresh tokens.
/// </summary>
public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly EdificiaDbContext _context;

    public RefreshTokenRepository(EdificiaDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
