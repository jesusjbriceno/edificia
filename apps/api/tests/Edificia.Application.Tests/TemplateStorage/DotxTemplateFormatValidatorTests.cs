using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Edificia.Infrastructure.TemplateStorage;
using FluentAssertions;

namespace Edificia.Application.Tests.TemplateStorage;

public class DotxTemplateFormatValidatorTests
{
    private readonly DotxTemplateFormatValidator _validator = new();

    [Fact]
    public void Validate_ShouldSucceed_WhenMemoriaTecnicaHasRequiredTags()
    {
        var bytes = CreateTemplateWithTags("ProjectTitle", "MD.01", "MC.01", "MD.02");

        var result = _validator.Validate("MemoriaTecnica", bytes);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenMemoriaTecnicaMissingRequiredTags()
    {
        var bytes = CreateTemplateWithTags("ProjectTitle", "MD.02");

        var result = _validator.Validate("MemoriaTecnica", bytes);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Template.InvalidFormat");
        result.Error.Description.Should().Contain("MC.01");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTemplateHasNoTags()
    {
        var bytes = CreateTemplateWithoutTags();

        var result = _validator.Validate("MemoriaTecnica", bytes);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Template.InvalidFormat");
        result.Error.Description.Should().Contain("Tag");
    }

    [Fact]
    public void Validate_ShouldFail_WhenBinaryIsNotOpenXml()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5 };

        var result = _validator.Validate("MemoriaTecnica", bytes);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Template.InvalidFormat");
    }

    private static byte[] CreateTemplateWithTags(params string[] tags)
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Template, true))
        {
            var mainPart = document.AddMainDocumentPart();
            var body = new Body();

            foreach (var tag in tags)
            {
                body.Append(CreateTaggedBlock(tag));
            }

            mainPart.Document = new Document(body);
            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    private static byte[] CreateTemplateWithoutTags()
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Template, true))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document(
                new Body(
                    new Paragraph(new Run(new Text("Plantilla sin controles")))));
            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    private static SdtBlock CreateTaggedBlock(string tag)
    {
        return new SdtBlock(
            new SdtProperties(
                new SdtAlias { Val = tag },
                new Tag { Val = tag },
                new SdtContentText()),
            new SdtContentBlock(
                new Paragraph(
                    new Run(
                        new Text($"<<{tag}>>")))));
    }
}
