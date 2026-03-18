using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Infrastructure.Persistence.Repositories;

public sealed class TemplateParamRepository : BaseRepository<TemplateParam>, ITemplateParamRepository
{
    public TemplateParamRepository(EdificiaDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TemplateParam>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Key)
            .ToListAsync(cancellationToken);
    }
}
