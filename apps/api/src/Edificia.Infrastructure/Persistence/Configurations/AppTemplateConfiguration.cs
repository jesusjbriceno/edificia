using Edificia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edificia.Infrastructure.Persistence.Configurations;

public sealed class AppTemplateConfiguration : IEntityTypeConfiguration<AppTemplate>
{
    public void Configure(EntityTypeBuilder<AppTemplate> builder)
    {
        builder.ToTable("app_templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.TemplateType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.MimeType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.FileSizeBytes)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.Version)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(t => t.CreatedByUserId)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasIndex(t => new { t.TemplateType, t.IsActive });
        builder.HasIndex(t => new { t.TemplateType, t.Version })
            .IsUnique();
        builder.HasIndex(t => t.TemplateType)
            .HasFilter("is_active = true")
            .IsUnique();
        builder.HasIndex(t => t.CreatedByUserId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_AppTemplates_FileSizeBytes_Positive", "file_size_bytes > 0");
            t.HasCheckConstraint("CK_AppTemplates_Version_Positive", "version > 0");
        });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
