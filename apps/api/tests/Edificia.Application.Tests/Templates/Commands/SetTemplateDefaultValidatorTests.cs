using Edificia.Application.Templates.Commands.SetTemplateDefault;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class SetTemplateDefaultValidatorTests
{
    private readonly SetTemplateDefaultValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenTemplateIdIsValid()
    {
        var command = new SetTemplateDefaultCommand(Guid.NewGuid(), true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenTemplateIdIsEmpty()
    {
        var command = new SetTemplateDefaultCommand(Guid.Empty, true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TemplateId");
    }
}