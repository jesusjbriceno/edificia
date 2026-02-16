using Edificia.Application.Projects.Commands.PatchSectionContent;
using FluentAssertions;

namespace Edificia.Application.Tests.Projects.Commands;

public class PatchSectionContentMappingTests
{
    [Fact]
    public void Create_ShouldMapAllFields()
    {
        var projectId = Guid.NewGuid();
        const string sectionId = "CTE-DB-SI-1";
        var request = new UpdateSectionRequest("<p>Contenido actualizado</p>");

        var command = PatchSectionContentCommand.Create(projectId, sectionId, request);

        command.ProjectId.Should().Be(projectId);
        command.SectionId.Should().Be(sectionId);
        command.Content.Should().Be("<p>Contenido actualizado</p>");
    }
}
