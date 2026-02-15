using Edificia.Domain.Entities;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Repository interface for Project aggregate (write-side).
/// Defined in Application layer; implemented in Infrastructure.
/// </summary>
public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
