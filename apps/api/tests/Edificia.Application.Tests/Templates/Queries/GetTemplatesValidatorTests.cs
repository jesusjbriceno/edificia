using Edificia.Application.Templates.Queries.GetTemplates;
using FluentAssertions;

namespace Edificia.Application.Tests.Templates.Queries;

public class GetTemplatesValidatorTests
{
    private readonly GetTemplatesValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenTemplateTypeIsNull()
    {
        var query = new GetTemplatesQuery(null, null);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldPass_WhenTemplateTypeLengthIsValid()
    {
        var query = new GetTemplatesQuery("MemoriaTecnica", null);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WhenTemplateTypeExceedsMaxLength()
    {
        var query = new GetTemplatesQuery(new string('X', 101), null);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TemplateType");
    }
}
