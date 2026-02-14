using Edificia.Domain.Entities;
using Edificia.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Edificia. Uses PostgreSQL with snake_case naming convention.
/// Write-side of CQRS (Commands use this context).
/// </summary>
public class EdificiaDbContext : DbContext
{
    public EdificiaDbContext(DbContextOptions<EdificiaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EdificiaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Automatically sets CreatedAt/UpdatedAt on auditable entities.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
