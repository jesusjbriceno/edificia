using Edificia.Domain.Primitives;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Generic base repository interface for aggregate roots (write-side).
/// Provides standard CRUD operations for any entity that extends <see cref="Entity"/>.
/// Defined in Application layer; implemented in Infrastructure.
/// </summary>
/// <typeparam name="T">The entity type, must extend <see cref="Entity"/>.</typeparam>
public interface IBaseRepository<T> where T : Entity
{
    /// <summary>
    /// Gets an entity by its unique identifier.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the data store.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
