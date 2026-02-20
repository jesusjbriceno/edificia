using Edificia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edificia.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Project entity.
/// Maps to PostgreSQL table "projects" with snake_case columns.
/// </summary>
public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Address)
            .HasMaxLength(500);

        builder.Property(p => p.InterventionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.IsLoeRequired)
            .IsRequired();

        builder.Property(p => p.CadastralReference)
            .HasMaxLength(100);

        builder.Property(p => p.LocalRegulations)
            .HasMaxLength(5000);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.ContentTreeJson)
            .HasColumnType("jsonb");

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Ownership: CreatedByUserId FK
        builder.Property(p => p.CreatedByUserId)
            .IsRequired();

        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Members navigation
        builder.HasMany(p => p.Members)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.CreatedByUserId);
    }
}
