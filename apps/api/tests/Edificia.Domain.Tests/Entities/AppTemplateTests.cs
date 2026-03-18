using Edificia.Domain.Entities;
using Edificia.Domain.Exceptions;
using FluentAssertions;

namespace Edificia.Domain.Tests.Entities;

public class AppTemplateTests
{
    [Fact]
    public void Create_ShouldInitializeTemplateWithExpectedDefaults()
    {
        var createdByUserId = Guid.NewGuid();

        var template = AppTemplate.Create(
            name: "Plantilla Memoria Base",
            description: "Plantilla corporativa",
            templateType: "MemoriaTecnica",
            storagePath: "templates/memoria/v1.dotx",
            originalFileName: "Memoria_v1.dotx",
            mimeType: "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            fileSizeBytes: 245_781,
            createdByUserId: createdByUserId);

        template.Id.Should().NotBeEmpty();
        template.Name.Should().Be("Plantilla Memoria Base");
        template.Description.Should().Be("Plantilla corporativa");
        template.TemplateType.Should().Be("MemoriaTecnica");
        template.StoragePath.Should().Be("templates/memoria/v1.dotx");
        template.OriginalFileName.Should().Be("Memoria_v1.dotx");
        template.MimeType.Should().Be("application/vnd.openxmlformats-officedocument.wordprocessingml.template");
        template.FileSizeBytes.Should().Be(245_781);
        template.CreatedByUserId.Should().Be(createdByUserId);
        template.IsActive.Should().BeFalse();
        template.Version.Should().Be(1);
    }

    [Fact]
    public void Activate_ShouldSetTemplateAsActive()
    {
        var template = CreateTemplate();

        template.Activate();

        template.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowBusinessRuleException()
    {
        var template = CreateTemplate();
        template.Activate();

        var act = template.Activate;

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*ya está activa*");
    }

    [Fact]
    public void Deactivate_ShouldSetTemplateAsInactive()
    {
        var template = CreateTemplate();
        template.Activate();

        template.Deactivate();

        template.IsActive.Should().BeFalse();
    }

    [Fact]
    public void PublishNewVersion_ShouldUpdateBinaryMetadata_IncrementVersion_AndActivate()
    {
        var template = CreateTemplate();

        template.PublishNewVersion(
            storagePath: "templates/memoria/v2.dotx",
            originalFileName: "Memoria_v2.dotx",
            mimeType: "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            fileSizeBytes: 267_102);

        template.StoragePath.Should().Be("templates/memoria/v2.dotx");
        template.OriginalFileName.Should().Be("Memoria_v2.dotx");
        template.FileSizeBytes.Should().Be(267_102);
        template.Version.Should().Be(2);
        template.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Rename_ShouldUpdateNameAndDescription()
    {
        var template = CreateTemplate();

        template.Rename("Plantilla Memoria Oficial", "Nueva descripción");

        template.Name.Should().Be("Plantilla Memoria Oficial");
        template.Description.Should().Be("Nueva descripción");
    }

    private static AppTemplate CreateTemplate()
    {
        return AppTemplate.Create(
            name: "Plantilla Memoria Base",
            description: "Plantilla corporativa",
            templateType: "MemoriaTecnica",
            storagePath: "templates/memoria/v1.dotx",
            originalFileName: "Memoria_v1.dotx",
            mimeType: "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            fileSizeBytes: 245_781,
            createdByUserId: Guid.NewGuid());
    }
}
