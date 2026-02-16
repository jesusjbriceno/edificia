using Edificia.Application.Projects.Commands.UpdateProjectTree;
using FluentAssertions;

namespace Edificia.Application.Tests.Projects.Commands;

public class UpdateProjectTreeMappingTests
{
    [Fact]
    public void Create_ShouldMapProjectIdAndContentTreeJson()
    {
        var projectId = Guid.NewGuid();
        var request = new UpdateProjectTreeRequest("{\"sections\":[]}");

        var command = UpdateProjectTreeCommand.Create(projectId, request);

        command.ProjectId.Should().Be(projectId);
        command.ContentTreeJson.Should().Be("{\"sections\":[]}");
    }
}
