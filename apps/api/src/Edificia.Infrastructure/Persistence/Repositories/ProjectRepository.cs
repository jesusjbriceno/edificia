using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IProjectRepository (write-side).
/// </summary>
public sealed class ProjectRepository : IProjectRepository
{
    private readonly EdificiaDbContext _context;

    public ProjectRepository(EdificiaDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
