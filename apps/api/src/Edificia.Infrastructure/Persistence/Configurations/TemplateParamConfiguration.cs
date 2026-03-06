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
        });
    }
}
