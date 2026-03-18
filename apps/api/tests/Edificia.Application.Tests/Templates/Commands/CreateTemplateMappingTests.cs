using Edificia.Application.Templates.Commands.CreateTemplate;
using Edificia.Application.Templates.DTOs;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class CreateTemplateMappingTests
{
    [Fact]
    public void CreateFactory_ShouldMapAllFields()
    {
        var userId = Guid.NewGuid();
        var fileBytes = new byte[] { 1, 2, 3 };
        var request = new CreateTemplateRequest("Plantilla Memoria", "MemoriaTecnica", "Descripción");

        var command = CreateTemplateCommand.Create(
            userId,
            request,
            "plantilla.dotx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            fileBytes.Length,
            fileBytes);

        command.Name.Should().Be("Plantilla Memoria");
        command.TemplateType.Should().Be("MemoriaTecnica");
        command.Description.Should().Be("Descripción");
        command.OriginalFileName.Should().Be("plantilla.dotx");
        command.FileSizeBytes.Should().Be(3);
        command.FileContent.Should().Equal(fileBytes);
        command.CreatedByUserId.Should().Be(userId);
    }
}
