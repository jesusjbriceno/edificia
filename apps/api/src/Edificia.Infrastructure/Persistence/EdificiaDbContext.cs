using Edificia.Domain.Entities;
using Edificia.Domain.Primitives;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Edificia. Uses PostgreSQL with snake_case naming convention.
/// Extends IdentityDbContext to support ASP.NET Core Identity (users, roles, claims).
/// Write-side of CQRS (Commands use this context).
/// </summary>
public class EdificiaDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public EdificiaDbContext(DbContextOptions<EdificiaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply Identity schema first
        base.OnModelCreating(modelBuilder);

        // Rename Identity tables to snake_case convention
        modelBuilder.Entity<ApplicationUser>(b => b.ToTable("asp_net_users"));
        modelBuilder.Entity<IdentityRole<Guid>>(b => b.ToTable("asp_net_roles"));
        modelBuilder.Entity<IdentityUserRole<Guid>>(b => b.ToTable("asp_net_user_roles"));
        modelBuilder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("asp_net_user_claims"));
        modelBuilder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("asp_net_user_logins"));
        modelBuilder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("asp_net_user_tokens"));
        modelBuilder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("asp_net_role_claims"));

        // Configure ApplicationUser additional properties
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(u => u.CollegiateNumber)
                .HasMaxLength(50);

            b.Property(u => u.MustChangePassword)
                .IsRequired()
                .HasDefaultValue(false);

            b.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        });

        // Apply all IEntityTypeConfiguration from this assembly (Project, etc.)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EdificiaDbContext).Assembly);
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
