using System.Linq.Expressions;
using Edificia.Application.Common;
using Edificia.Domain.Primitives;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Generic base repository interface for aggregate roots (write-side).
/// Provides standard CRUD and query operations for any entity that extends <see cref="Entity"/>.
/// Defined in Application layer; implemented in Infrastructure.
/// </summary>
/// <typeparam name="T">The entity type, must extend <see cref="Entity"/>.</typeparam>
public interface IBaseRepository<T> where T : Entity
{
    // ──────────────────── Reads ────────────────────

    /// <summary>
    /// Gets an entity by its unique identifier.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities of this type.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities matching a predicate.
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity matching a predicate, or null.
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an entity with the given ID exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether any entity matches the predicate.
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total count of entities.
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the count of entities matching a predicate.
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of entities with optional filtering and ordering.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="predicate">Optional filter expression.</param>
    /// <param name="orderBy">Optional ordering expression.</param>
    /// <param name="ascending">Sort direction (default: true).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PagedResponse<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    // ──────────────────── Writes ────────────────────

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a collection of entities to the repository.
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an entity as modified in the change tracker.
    /// Changes are persisted on <see cref="SaveChangesAsync"/>.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Marks an entity for deletion.
    /// Deletion is executed on <see cref="SaveChangesAsync"/>.
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Marks a collection of entities for deletion.
    /// </summary>
    void RemoveRange(IEnumerable<T> entities);

    // ──────────────────── Persistence ────────────────────

    /// <summary>
    /// Persists all pending changes to the data store.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
