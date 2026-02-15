using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;

namespace Edificia.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IProjectRepository (write-side).
/// Inherits standard CRUD from <see cref="BaseRepository{T}"/>.
/// Add Project-specific queries here as needed.
/// </summary>
public sealed class ProjectRepository : BaseRepository<Project>, IProjectRepository
{
    public ProjectRepository(EdificiaDbContext context) : base(context)
    {
    }
}
