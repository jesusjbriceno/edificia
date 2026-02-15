using Edificia.Domain.Entities;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Repository interface for Project aggregate (write-side).
/// Extends <see cref="IBaseRepository{T}"/> with Project-specific operations.
/// Defined in Application layer; implemented in Infrastructure.
/// </summary>
public interface IProjectRepository : IBaseRepository<Project>
{
}
