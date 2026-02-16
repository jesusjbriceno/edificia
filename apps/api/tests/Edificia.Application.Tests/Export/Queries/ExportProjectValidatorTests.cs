using Edificia.Application.Export.Queries.ExportProject;
using FluentAssertions;

namespace Edificia.Application.Tests.Export.Queries;

public class ExportProjectValidatorTests
{
    private readonly ExportProjectValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenProjectIdIsValid()
    {
        var query = new ExportProjectQuery(Guid.NewGuid());

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenProjectIdIsEmpty()
    {
        var query = new ExportProjectQuery(Guid.Empty);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be("ProjectId");
    }
}
