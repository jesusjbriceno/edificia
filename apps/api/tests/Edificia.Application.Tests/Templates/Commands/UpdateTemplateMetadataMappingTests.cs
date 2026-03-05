using Edificia.Application.Templates.Commands.UpdateTemplateMetadata;
using Edificia.Application.Templates.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class UpdateTemplateMetadataMappingTests
{
    [Fact]
    public void CreateFactory_ShouldMapFields()
    {
        var templateId = Guid.NewGuid();
        var request = new UpdateTemplateMetadataRequest("Nueva plantilla", "Descripción actualizada");

        var command = UpdateTemplateMetadataCommand.Create(templateId, request);

        command.TemplateId.Should().Be(templateId);
        command.Name.Should().Be("Nueva plantilla");
        command.Description.Should().Be("Descripción actualizada");
    }
}