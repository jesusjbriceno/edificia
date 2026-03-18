using Edificia.Application.Templates.Commands.UpdateTemplateMetadata;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class UpdateTemplateMetadataValidatorTests
{
    private readonly UpdateTemplateMetadataValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenCommandIsValid()
    {
        var command = new UpdateTemplateMetadataCommand(Guid.NewGuid(), "Plantilla", "Descripción");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenTemplateIdIsEmpty()
    {
        var command = new UpdateTemplateMetadataCommand(Guid.Empty, "Plantilla", null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TemplateId");
    }

    [Fact]
    public void ShouldFail_WhenNameIsEmpty()
    {
        var command = new UpdateTemplateMetadataCommand(Guid.NewGuid(), string.Empty, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }
}