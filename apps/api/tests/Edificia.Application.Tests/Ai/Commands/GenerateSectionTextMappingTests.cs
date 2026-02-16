using Edificia.Application.Ai.Commands.GenerateSectionText;
using FluentAssertions;

namespace Edificia.Application.Tests.Ai.Commands;

public class GenerateSectionTextMappingTests
{
    [Fact]
    public void Create_ShouldMapAllFields()
    {
        var projectId = Guid.NewGuid();
        var request = new GenerateTextRequest("CTE-DB-SI-1", "Genera la sección", "Contexto adicional");

        var command = GenerateSectionTextCommand.Create(projectId, request);

        command.ProjectId.Should().Be(projectId);
        command.SectionId.Should().Be("CTE-DB-SI-1");
        command.Prompt.Should().Be("Genera la sección");
        command.Context.Should().Be("Contexto adicional");
    }

    [Fact]
    public void Create_ShouldMapNullContext()
    {
        var projectId = Guid.NewGuid();
        var request = new GenerateTextRequest("SEC-01", "Prompt", null);

        var command = GenerateSectionTextCommand.Create(projectId, request);

        command.ProjectId.Should().Be(projectId);
        command.Context.Should().BeNull();
    }
}
