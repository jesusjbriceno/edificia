using Edificia.Application.Ai.Commands.GenerateSectionText;
using FluentAssertions;

namespace Edificia.Application.Tests.Ai.Commands;

public class GenerateSectionTextMappingTests
{
    [Fact]
    public void ExplicitOperator_ShouldMapDtoFields()
    {
        var request = new GenerateTextRequest("CTE-DB-SI-1", "Genera la sección", "Contexto adicional");

        var command = (GenerateSectionTextCommand)request;

        command.ProjectId.Should().Be(Guid.Empty);
        command.SectionId.Should().Be("CTE-DB-SI-1");
        command.Prompt.Should().Be("Genera la sección");
        command.Context.Should().Be("Contexto adicional");
    }

    [Fact]
    public void ExplicitOperator_EnrichedWithRouteContext_ShouldSetProjectId()
    {
        var projectId = Guid.NewGuid();
        var request = new GenerateTextRequest("SEC-01", "Prompt", null);

        var command = (GenerateSectionTextCommand)request with { ProjectId = projectId };

        command.ProjectId.Should().Be(projectId);
        command.Context.Should().BeNull();
    }
}
