using Edificia.Application.Interfaces;
using Edificia.Application.Projects.Commands.PatchSectionContent;
using Edificia.Domain.Entities;
using Edificia.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Edificia.Application.Tests.Projects.Commands;

public class PatchSectionContentHandlerTests
{
    private readonly Mock<IProjectRepository> _repositoryMock;
    private readonly PatchSectionContentHandler _handler;

    private const string SampleTree = """
    {
        "chapters": [
            {
                "id": "md",
                "title": "Memoria Descriptiva",
                "content": "",
                "sections": [
                    {
                        "id": "md-1",
                        "title": "Agentes",
                        "content": "<p>Contenido original</p>"
                    }
                ]
            }
        ]
    }
    """;

    public PatchSectionContentHandlerTests()
    {
        _repositoryMock = new Mock<IProjectRepository>();
        _handler = new PatchSectionContentHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenSectionUpdated()
    {
        var projectId = Guid.NewGuid();
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        project.UpdateContentTree(SampleTree);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new PatchSectionContentCommand(projectId, "md-1", "<p>Actualizado</p>");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProjectDoesNotExist()
    {
        var projectId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var command = new PatchSectionContentCommand(projectId, "md-1", "<p>Algo</p>");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("NotFound");
        result.Error.Description.Should().Contain(projectId.ToString());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenContentTreeIsNull()
    {
        var projectId = Guid.NewGuid();
        var project = Project.Create("Test", InterventionType.Reform, false, Guid.NewGuid());
        // ContentTreeJson is null by default

        _repositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new PatchSectionContentCommand(projectId, "md-1", "<p>Algo</p>");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSectionNotFound()
    {
        var projectId = Guid.NewGuid();
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        project.UpdateContentTree(SampleTree);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new PatchSectionContentCommand(projectId, "nonexistent-section", "<p>Algo</p>");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("NotFound");
        result.Error.Description.Should().Contain("nonexistent-section");
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenSectionUpdated()
    {
        var projectId = Guid.NewGuid();
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        project.UpdateContentTree(SampleTree);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new PatchSectionContentCommand(projectId, "md-1", "<p>Actualizado</p>");
        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChanges_WhenProjectNotFound()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var command = new PatchSectionContentCommand(Guid.NewGuid(), "md-1", "<p>Algo</p>");
        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChanges_WhenSectionNotFound()
    {
        var projectId = Guid.NewGuid();
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        project.UpdateContentTree(SampleTree);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new PatchSectionContentCommand(projectId, "nonexistent", "<p>Algo</p>");
        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateContentTree_OnProject()
    {
        var projectId = Guid.NewGuid();
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        project.UpdateContentTree(SampleTree);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new PatchSectionContentCommand(projectId, "md-1", "<p>Nuevo contenido</p>");
        await _handler.Handle(command, CancellationToken.None);

        project.ContentTreeJson.Should().Contain("<p>Nuevo contenido</p>");
        project.ContentTreeJson.Should().NotContain("<p>Contenido original</p>");
    }
}
