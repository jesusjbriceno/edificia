using Edificia.Application.Templates.Commands.ToggleTemplateStatus;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class ToggleTemplateStatusValidatorTests
{
    private readonly ToggleTemplateStatusValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenTemplateIdIsValid()
    {
        var command = new ToggleTemplateStatusCommand(Guid.NewGuid(), true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenTemplateIdIsEmpty()
    {
        var command = new ToggleTemplateStatusCommand(Guid.Empty, true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TemplateId");
    }
}
