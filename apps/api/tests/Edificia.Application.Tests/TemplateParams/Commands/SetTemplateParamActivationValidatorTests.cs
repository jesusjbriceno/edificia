using Edificia.Application.TemplateParams.Commands.SetTemplateParamActivation;
using FluentAssertions;

namespace Edificia.Application.Tests.TemplateParams.Commands;

public class SetTemplateParamActivationValidatorTests
{
    private readonly SetTemplateParamActivationValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenTemplateParamIdIsValid()
    {
        var command = new SetTemplateParamActivationCommand(Guid.NewGuid(), true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenTemplateParamIdIsEmpty()
    {
        var command = new SetTemplateParamActivationCommand(Guid.Empty, true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TemplateParamId");
    }
}
