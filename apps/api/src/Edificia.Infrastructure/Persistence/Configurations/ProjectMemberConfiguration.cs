using Edificia.Domain.Entities;
using Edificia.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edificia.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ProjectMember entity.
/// Maps to PostgreSQL table "project_members" with snake_case columns.
/// </summary>
public sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("project_members");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.ProjectId)
            .IsRequired();

        builder.Property(pm => pm.UserId)
            .IsRequired();

        builder.Property(pm => pm.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(pm => pm.CreatedAt)
            .IsRequired();

        // FK to ApplicationUser
        builder.HasOne(pm => pm.User)
            .WithMany()
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK to Project is configured in ProjectConfiguration (HasMany)

        // Unique constraint: one role per user per project
        builder.HasIndex(pm => new { pm.ProjectId, pm.UserId })
            .IsUnique();

        // Indexes for queries
        builder.HasIndex(pm => pm.UserId);
        builder.HasIndex(pm => pm.Role);
    }
}
