using Edificia.Application.Interfaces;
using Edificia.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic EF Core implementation of <see cref="IBaseRepository{T}"/> (write-side).
/// Provides standard CRUD operations backed by <see cref="EdificiaDbContext"/>.
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

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}
