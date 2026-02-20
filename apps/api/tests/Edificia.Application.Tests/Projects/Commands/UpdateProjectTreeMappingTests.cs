using Edificia.Application.Projects.Commands.UpdateProjectTree;
using FluentAssertions;

namespace Edificia.Application.Tests.Projects.Commands;

public class UpdateProjectTreeMappingTests
{
    [Fact]
    public void ExplicitOperator_ShouldMapDtoFields()
    {
        var request = new UpdateProjectTreeRequest("{\"sections\":[]}");

        var command = (UpdateProjectTreeCommand)request;

        command.ProjectId.Should().Be(Guid.Empty);
        command.ContentTreeJson.Should().Be("{\"sections\":[]}");
    }

    [Fact]
    public void ExplicitOperator_EnrichedWithRouteContext_ShouldSetProjectId()
    {
        var projectId = Guid.NewGuid();
        var request = new UpdateProjectTreeRequest("{\"sections\":[]}");

        var command = (UpdateProjectTreeCommand)request with { ProjectId = projectId };

        command.ProjectId.Should().Be(projectId);
        command.ContentTreeJson.Should().Be("{\"sections\":[]}");
    }
}
