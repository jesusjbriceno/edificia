using Edificia.Application.Projects.Commands.PatchSectionContent;
using FluentAssertions;

namespace Edificia.Application.Tests.Projects.Commands;

public class PatchSectionContentValidatorTests
{
    private readonly PatchSectionContentValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenAllFieldsAreValid()
    {
        var command = new PatchSectionContentCommand(
            Guid.NewGuid(),
            "md-1",
            "<p>Contenido actualizado</p>");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenProjectIdIsEmpty()
    {
        var command = new PatchSectionContentCommand(
            Guid.Empty,
            "md-1",
            "<p>Contenido</p>");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "ProjectId" &&
            e.ErrorMessage.Contains("obligatorio"));
    }

    [Fact]
    public void ShouldFail_WhenSectionIdIsEmpty()
    {
        var command = new PatchSectionContentCommand(
            Guid.NewGuid(),
            "",
            "<p>Contenido</p>");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "SectionId" &&
            e.ErrorMessage.Contains("obligatorio"));
    }

    [Fact]
    public void ShouldFail_WhenSectionIdIsNull()
    {
        var command = new PatchSectionContentCommand(
            Guid.NewGuid(),
            null!,
            "<p>Contenido</p>");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "SectionId");
    }

    [Fact]
    public void ShouldFail_WhenContentIsNull()
    {
        var command = new PatchSectionContentCommand(
            Guid.NewGuid(),
            "md-1",
            null!);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Content" &&
            e.ErrorMessage.Contains("obligatorio"));
    }

    [Fact]
    public void ShouldPass_WhenContentIsEmpty()
    {
        var command = new PatchSectionContentCommand(
            Guid.NewGuid(),
            "md-1",
            "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenContentExceedsMaxLength()
    {
        var oversizedContent = new string('A', 524_289); // 512 KB + 1

        var command = new PatchSectionContentCommand(
            Guid.NewGuid(),
            "md-1",
            oversizedContent);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Content" &&
            e.ErrorMessage.Contains("KB"));
    }

    [Fact]
    public void ShouldPass_WhenContentIsAtMaxLength()
    {
        var maxContent = new string('A', 524_288); // Exactly 512 KB

        var command = new PatchSectionContentCommand(
            Guid.NewGuid(),
            "md-1",
            maxContent);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenSectionIdExceedsMaxLength()
    {
        var longSectionId = new string('x', 201);

        var command = new PatchSectionContentCommand(
            Guid.NewGuid(),
            longSectionId,
            "<p>Contenido</p>");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "SectionId" &&
            e.ErrorMessage.Contains("caracteres"));
    }

    [Fact]
    public void ShouldPass_WithMinimalValidCommand()
    {
        var command = new PatchSectionContentCommand(
            Guid.NewGuid(),
            "a",
            "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
