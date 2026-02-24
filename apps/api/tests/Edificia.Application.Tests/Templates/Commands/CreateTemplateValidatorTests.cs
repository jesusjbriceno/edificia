using Edificia.Application.Templates.Commands.CreateTemplate;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class CreateTemplateValidatorTests
{
    private readonly CreateTemplateValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenRequestIsValid()
    {
        var command = BuildValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenFileExtensionIsNotDotx()
    {
        var command = BuildValidCommand() with { OriginalFileName = "plantilla.docx" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OriginalFileName");
    }

    [Fact]
    public void ShouldFail_WhenMimeTypeIsInvalid()
    {
        var command = BuildValidCommand() with { MimeType = "application/pdf" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MimeType");
    }

    [Fact]
    public void ShouldFail_WhenFileIsTooLarge()
    {
        var fileBytes = new byte[10 * 1024 * 1024 + 1];
        var command = BuildValidCommand() with
        {
            FileContent = fileBytes,
            FileSizeBytes = fileBytes.Length
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FileSizeBytes");
    }

    [Fact]
    public void ShouldFail_WhenFileContentLengthDoesNotMatchSize()
    {
        var command = BuildValidCommand() with { FileSizeBytes = 999 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FileContent");
    }

    private static CreateTemplateCommand BuildValidCommand()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };

        return new CreateTemplateCommand(
            Name: "Plantilla Base",
            TemplateType: "MemoriaTecnica",
            Description: "Descripción",
            OriginalFileName: "plantilla.dotx",
            MimeType: "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            FileSizeBytes: bytes.Length,
            FileContent: bytes,
            CreatedByUserId: Guid.NewGuid());
    }
}
