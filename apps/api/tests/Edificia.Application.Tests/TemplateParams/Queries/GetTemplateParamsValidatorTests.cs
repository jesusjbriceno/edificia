using Edificia.Application.TemplateParams.Queries.GetTemplateParams;
using FluentAssertions;

namespace Edificia.Application.Tests.TemplateParams.Queries;

public class GetTemplateParamsValidatorTests
{
    private readonly GetTemplateParamsValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenIsActiveIsNull()
    {
        var query = new GetTemplateParamsQuery(null);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldPass_WhenIsActiveIsProvided()
    {
        var query = new GetTemplateParamsQuery(true);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }
}
