using System.Linq.Expressions;
using Edificia.Application.Common;
using Edificia.Application.Interfaces;
using Edificia.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic EF Core implementation of <see cref="IBaseRepository{T}"/> (write-side).
/// Provides standard CRUD and query operations backed by <see cref="EdificiaDbContext"/>.
/// </summary>
/// <typeparam name="T">The entity type, must extend <see cref="Entity"/>.</typeparam>
public abstract class BaseRepository<T> : IBaseRepository<T> where T : Entity
{
    protected readonly EdificiaDbContext Context;

    protected BaseRepository(EdificiaDbContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Returns the <see cref="DbSet{T}"/> for the entity type.
    /// </summary>
    protected DbSet<T> DbSet => Context.Set<T>();

    // ──────────────────── Reads ────────────────────

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<PagedResponse<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = DbSet.AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        var totalCount = await query.CountAsync(cancellationToken);

        query = orderBy is not null
            ? ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy)
            : query.OrderByDescending(e => e.Id); // Default: deterministic order

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<T>(items, totalCount, page, pageSize);
    }

    // ──────────────────── Writes ────────────────────

    /// <inheritdoc />
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    /// <inheritdoc />
    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    /// <inheritdoc />
    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }

    // ──────────────────── Persistence ────────────────────

    /// <inheritdoc />
    public virtual Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}
