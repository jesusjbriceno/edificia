using Edificia.Application.Projects.Commands.UpdateProjectTree;
using FluentAssertions;

namespace Edificia.Application.Tests.Projects.Commands;

public class UpdateProjectTreeValidatorTests
{
    private readonly UpdateProjectTreeValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenAllFieldsAreValid()
    {
        var command = new UpdateProjectTreeCommand(
            Guid.NewGuid(),
            """{"chapters": [{"id": "1", "title": "Memoria Descriptiva"}]}""");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenProjectIdIsEmpty()
    {
        var command = new UpdateProjectTreeCommand(
            Guid.Empty,
            "{}");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "ProjectId" &&
            e.ErrorMessage.Contains("obligatorio"));
    }

    [Fact]
    public void ShouldFail_WhenContentTreeJsonIsEmpty()
    {
        var command = new UpdateProjectTreeCommand(
            Guid.NewGuid(),
            "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "ContentTreeJson" &&
            e.ErrorMessage.Contains("obligatorio"));
    }

    [Fact]
    public void ShouldFail_WhenContentTreeJsonIsNull()
    {
        var command = new UpdateProjectTreeCommand(
            Guid.NewGuid(),
            null!);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "ContentTreeJson");
    }

    [Fact]
    public void ShouldFail_WhenContentTreeJsonExceedsMaxLength()
    {
        var oversizedContent = new string('A', 1_048_577); // 1 MB + 1

        var command = new UpdateProjectTreeCommand(
            Guid.NewGuid(),
            oversizedContent);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "ContentTreeJson" &&
            e.ErrorMessage.Contains("KB"));
    }

    [Fact]
    public void ShouldPass_WhenContentTreeJsonIsAtMaxLength()
    {
        var maxContent = new string('A', 1_048_576); // Exactly 1 MB

        var command = new UpdateProjectTreeCommand(
            Guid.NewGuid(),
            maxContent);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldPass_WithMinimalValidJson()
    {
        var command = new UpdateProjectTreeCommand(
            Guid.NewGuid(),
            "{}");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
