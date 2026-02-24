using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of ITemplateRepository (write-side).
/// </summary>
public sealed class TemplateRepository : BaseRepository<AppTemplate>, ITemplateRepository
{
    public TemplateRepository(EdificiaDbContext context) : base(context)
    {
    }

    public async Task<AppTemplate?> GetActiveByTypeAsync(
        string templateType,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.TemplateType == templateType && t.IsActive)
            .OrderByDescending(t => t.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountByTypeAsync(
        string templateType,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(t => t.TemplateType == templateType, cancellationToken);
    }
}
