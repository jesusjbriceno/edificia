using Edificia.Application.Ai.Commands.GenerateSectionText;
using FluentAssertions;

namespace Edificia.Application.Tests.Ai.Commands;

public class GenerateSectionTextValidatorTests
{
    private readonly GenerateSectionTextValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenAllFieldsAreValid()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(),
            "md-1",
            "Describe los agentes del proyecto",
            "Vivienda unifamiliar en Madrid");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenProjectIdIsEmpty()
    {
        var command = new GenerateSectionTextCommand(
            Guid.Empty,
            "md-1",
            "Prompt",
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "ProjectId" &&
            e.ErrorMessage.Contains("obligatorio"));
    }

    [Fact]
    public void ShouldFail_WhenSectionIdIsEmpty()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(),
            "",
            "Prompt",
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "SectionId" &&
            e.ErrorMessage.Contains("obligatorio"));
    }

    [Fact]
    public void ShouldFail_WhenSectionIdIsNull()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(),
            null!,
            "Prompt",
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "SectionId");
    }

    [Fact]
    public void ShouldFail_WhenPromptIsEmpty()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(),
            "md-1",
            "",
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Prompt" &&
            e.ErrorMessage.Contains("obligatorio"));
    }

    [Fact]
    public void ShouldFail_WhenPromptIsNull()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(),
            "md-1",
            null!,
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Prompt");
    }

    [Fact]
    public void ShouldFail_WhenPromptExceedsMaxLength()
    {
        var oversizedPrompt = new string('A', 10_001);

        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(),
            "md-1",
            oversizedPrompt,
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Prompt" &&
            e.ErrorMessage.Contains("caracteres"));
    }

    [Fact]
    public void ShouldPass_WhenPromptIsAtMaxLength()
    {
        var maxPrompt = new string('A', 10_000);

        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(),
            "md-1",
            maxPrompt,
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldPass_WhenContextIsNull()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(),
            "md-1",
            "Describe los agentes",
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldPass_WhenContextIsEmpty()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(),
            "md-1",
            "Describe los agentes",
            "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenContextExceedsMaxLength()
    {
        var oversizedContext = new string('A', 50_001);

        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(),
            "md-1",
            "Prompt",
            oversizedContext);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Context" &&
            e.ErrorMessage.Contains("caracteres"));
    }

    [Fact]
    public void ShouldFail_WhenSectionIdExceedsMaxLength()
    {
        var longSectionId = new string('x', 201);

        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(),
            longSectionId,
            "Prompt",
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "SectionId" &&
            e.ErrorMessage.Contains("caracteres"));
    }
}
