using Edificia.Application.Projects.Commands.PatchSectionContent;
using FluentAssertions;

namespace Edificia.Application.Tests.Projects.Commands;

public class PatchSectionContentMappingTests
{
    [Fact]
    public void ExplicitOperator_ShouldMapDtoField()
    {
        var request = new UpdateSectionRequest("<p>Contenido actualizado</p>");

        var command = (PatchSectionContentCommand)request;

        command.ProjectId.Should().Be(Guid.Empty);
        command.SectionId.Should().BeEmpty();
        command.Content.Should().Be("<p>Contenido actualizado</p>");
    }

    [Fact]
    public void ExplicitOperator_EnrichedWithRouteContext_ShouldSetProjectIdAndSectionId()
    {
        var projectId = Guid.NewGuid();
        const string sectionId = "CTE-DB-SI-1";
        var request = new UpdateSectionRequest("<p>Contenido actualizado</p>");

        var command = (PatchSectionContentCommand)request with { ProjectId = projectId, SectionId = sectionId };

        command.ProjectId.Should().Be(projectId);
        command.SectionId.Should().Be(sectionId);
        command.Content.Should().Be("<p>Contenido actualizado</p>");
    }
}
