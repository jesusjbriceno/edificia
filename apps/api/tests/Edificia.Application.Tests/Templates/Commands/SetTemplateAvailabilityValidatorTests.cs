using Edificia.Application.Templates.Commands.SetTemplateAvailability;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class SetTemplateAvailabilityValidatorTests
{
    private readonly SetTemplateAvailabilityValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenTemplateIdIsValid()
    {
        var command = new SetTemplateAvailabilityCommand(Guid.NewGuid(), true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenTemplateIdIsEmpty()
    {
        var command = new SetTemplateAvailabilityCommand(Guid.Empty, true);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TemplateId");
    }
}