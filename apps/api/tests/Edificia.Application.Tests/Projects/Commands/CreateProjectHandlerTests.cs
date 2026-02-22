using Edificia.Application.Interfaces;
using Edificia.Application.Projects.Commands.CreateProject;
using Edificia.Domain.Entities;
using Edificia.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Edificia.Application.Tests.Projects.Commands;

public class CreateProjectHandlerTests
{
    private readonly Mock<IProjectRepository> _repositoryMock;
    private readonly CreateProjectHandler _handler;

    public CreateProjectHandlerTests()
    {
        _repositoryMock = new Mock<IProjectRepository>();
        _handler = new CreateProjectHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithGuid()
    {
        var command = new CreateProjectCommand(
            "Vivienda Unifamiliar",
            InterventionType.NewConstruction,
            true,
            Guid.NewGuid(),
            "Descripción",
            "Calle Mayor 1");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallAddAndSaveChanges()
    {
        var command = new CreateProjectCommand(
            "Test",
            InterventionType.Reform,
            false,
            Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<Project>(p =>
                p.Title == "Test" &&
                p.InterventionType == InterventionType.Reform &&
                p.IsLoeRequired == false),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateProjectWithAllFields()
    {
        Project? capturedProject = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Callback<Project, CancellationToken>((p, _) => capturedProject = p);

        var command = new CreateProjectCommand(
            "Completo",
            InterventionType.Extension,
            true,
            Guid.NewGuid(),
            "Desc completa",
            "Calle X",
            "REF_CAT_123",
            "Normativa local");

        await _handler.Handle(command, CancellationToken.None);

        capturedProject.Should().NotBeNull();
        capturedProject!.Title.Should().Be("Completo");
        capturedProject.InterventionType.Should().Be(InterventionType.Extension);
        capturedProject.IsLoeRequired.Should().BeTrue();
        capturedProject.Description.Should().Be("Desc completa");
        capturedProject.Address.Should().Be("Calle X");
        capturedProject.CadastralReference.Should().Be("REF_CAT_123");
        capturedProject.LocalRegulations.Should().Be("Normativa local");
        capturedProject.Status.Should().Be(ProjectStatus.Draft);
    }
}
