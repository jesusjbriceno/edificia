using Edificia.Application.Templates.Commands.DeleteTemplate;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Commands;

public class DeleteTemplateValidatorTests
{
    private readonly DeleteTemplateValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenTemplateIdIsValid()
    {
        var command = new DeleteTemplateCommand(Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenTemplateIdIsEmpty()
    {
        var command = new DeleteTemplateCommand(Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TemplateId");
    }
}