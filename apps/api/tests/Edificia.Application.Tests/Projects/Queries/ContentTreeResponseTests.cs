using Edificia.Application.Projects.Queries.GetProjectTree;
using FluentAssertions;

namespace Edificia.Application.Tests.Projects.Queries;

public class ContentTreeResponseTests
{
    [Fact]
    public void ShouldCreateWithAllProperties()
    {
        var projectId = Guid.NewGuid();
        var contentTree = """{"chapters": [{"id": "1", "title": "Memoria Descriptiva"}]}""";

        var response = new ContentTreeResponse(
            projectId,
            "NewConstruction",
            true,
            contentTree);

        response.ProjectId.Should().Be(projectId);
        response.InterventionType.Should().Be("NewConstruction");
        response.IsLoeRequired.Should().BeTrue();
        response.ContentTreeJson.Should().Be(contentTree);
    }

    [Fact]
    public void ShouldAllowNullContentTreeJson()
    {
        var response = new ContentTreeResponse(
            Guid.NewGuid(),
            "Reform",
            false,
            null);

        response.ContentTreeJson.Should().BeNull();
    }

    [Fact]
    public void ShouldSupportReformInterventionType()
    {
        var response = new ContentTreeResponse(
            Guid.NewGuid(),
            "Reform",
            false,
            null);

        response.InterventionType.Should().Be("Reform");
        response.IsLoeRequired.Should().BeFalse();
    }

    [Fact]
    public void ShouldBeImmutableRecord()
    {
        var response1 = new ContentTreeResponse(Guid.NewGuid(), "NewConstruction", true, "{}");
        var response2 = response1 with { IsLoeRequired = false };

        response1.IsLoeRequired.Should().BeTrue();
        response2.IsLoeRequired.Should().BeFalse();
    }
}
