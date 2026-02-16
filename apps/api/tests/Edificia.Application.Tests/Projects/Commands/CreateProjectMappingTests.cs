using Edificia.Application.Projects.Commands.CreateProject;
using Edificia.Domain.Enums;
using FluentAssertions;

namespace Edificia.Application.Tests.Projects.Commands;

public class CreateProjectMappingTests
{
    [Fact]
    public void ExplicitOperator_ShouldMapAllFields()
    {
        var request = new CreateProjectRequest(
            "Vivienda Unifamiliar",
            InterventionType.NewConstruction,
            true,
            "Descripción",
            "Calle Mayor 1",
            "REF-001",
            "Normativa local");

        var command = (CreateProjectCommand)request;

        command.Title.Should().Be("Vivienda Unifamiliar");
        command.InterventionType.Should().Be(InterventionType.NewConstruction);
        command.IsLoeRequired.Should().BeTrue();
        command.Description.Should().Be("Descripción");
        command.Address.Should().Be("Calle Mayor 1");
        command.CadastralReference.Should().Be("REF-001");
        command.LocalRegulations.Should().Be("Normativa local");
    }

    [Fact]
    public void ExplicitOperator_ShouldMapNullOptionalFields()
    {
        var request = new CreateProjectRequest(
            "Reforma",
            InterventionType.Reform,
            false);

        var command = (CreateProjectCommand)request;

        command.Title.Should().Be("Reforma");
        command.InterventionType.Should().Be(InterventionType.Reform);
        command.IsLoeRequired.Should().BeFalse();
        command.Description.Should().BeNull();
        command.Address.Should().BeNull();
        command.CadastralReference.Should().BeNull();
        command.LocalRegulations.Should().BeNull();
    }
}
