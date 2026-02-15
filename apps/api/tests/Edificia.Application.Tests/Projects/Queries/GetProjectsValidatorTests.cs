using Edificia.Application.Projects.Queries.GetProjects;
using FluentAssertions;

namespace Edificia.Application.Tests.Projects.Queries;

public class GetProjectsValidatorTests
{
    private readonly GetProjectsValidator _validator = new();

    [Fact]
    public void ShouldPass_WithDefaultValues()
    {
        var query = new GetProjectsQuery();

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldPass_WithValidStatus()
    {
        var query = new GetProjectsQuery(Status: "Draft");

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    [InlineData("Archived")]
    public void ShouldPass_WithAllValidStatuses(string status)
    {
        var query = new GetProjectsQuery(Status: status);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldFail_WithInvalidStatus()
    {
        var query = new GetProjectsQuery(Status: "Invalid");

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Status" &&
            e.ErrorMessage.Contains("no es válido"));
    }

    [Fact]
    public void ShouldFail_WhenPageIsZero()
    {
        var query = new GetProjectsQuery(Page: 0);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Page");
    }

    [Fact]
    public void ShouldFail_WhenPageIsNegative()
    {
        var query = new GetProjectsQuery(Page: -1);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ShouldFail_WhenPageSizeIsZero()
    {
        var query = new GetProjectsQuery(PageSize: 0);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "PageSize");
    }

    [Fact]
    public void ShouldFail_WhenPageSizeExceedsMax()
    {
        var query = new GetProjectsQuery(PageSize: 51);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "PageSize" &&
            e.ErrorMessage.Contains("entre 1 y 50"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    public void ShouldPass_WithValidPageSizes(int pageSize)
    {
        var query = new GetProjectsQuery(PageSize: pageSize);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldPass_WithSearchTerm()
    {
        var query = new GetProjectsQuery(Search: "vivienda");

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldPass_WithNullStatus()
    {
        var query = new GetProjectsQuery(Status: null);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }
}
