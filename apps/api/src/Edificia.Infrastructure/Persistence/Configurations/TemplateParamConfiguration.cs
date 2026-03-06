using Edificia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edificia.Infrastructure.Persistence.Configurations;

public sealed class TemplateParamConfiguration : IEntityTypeConfiguration<TemplateParam>
{
    public void Configure(EntityTypeBuilder<TemplateParam> builder)
    {
        builder.ToTable("template_params");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.SourceCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Formatter)
            .HasMaxLength(50);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasIndex(p => p.Key)
            .IsUnique();

        builder.HasIndex(p => p.IsActive);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_TemplateParams_Key_NotEmpty", "key <> ''");
            t.HasCheckConstraint("CK_TemplateParams_SourceCode_NotEmpty", "source_code <> ''");
            t.HasCheckConstraint("CK_TemplateParams_Key_Format", "key ~ '^[A-Z0-9_]+$'");
            t.HasCheckConstraint(
                "CK_TemplateParams_SourceCode_Allowed",
                "source_code IN ('PROJECT_TITLE','PROJECT_DESCRIPTION','PROJECT_ADDRESS','INTERVENTION_TYPE','IS_LOE_REQUIRED','CADASTRAL_REFERENCE','LOCAL_REGULATIONS','EXPORT_DATE','EXPORT_DATETIME')");
            t.HasCheckConstraint(
                "CK_TemplateParams_Formatter_Allowed",
                "formatter IS NULL OR formatter IN ('UPPER','LOWER','TRIM')");
        });
    }
}
