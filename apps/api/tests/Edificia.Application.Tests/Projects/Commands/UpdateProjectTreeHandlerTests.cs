using Edificia.Application.Interfaces;
using Edificia.Application.Projects.Commands.UpdateProjectTree;
using Edificia.Domain.Entities;
using Edificia.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Edificia.Application.Tests.Projects.Commands;

public class UpdateProjectTreeHandlerTests
{
    private readonly Mock<IProjectRepository> _repositoryMock;
    private readonly UpdateProjectTreeHandler _handler;

    public UpdateProjectTreeHandlerTests()
    {
        _repositoryMock = new Mock<IProjectRepository>();
        _handler = new UpdateProjectTreeHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenProjectExists()
    {
        var projectId = Guid.NewGuid();
        var project = Project.Create("Test", InterventionType.NewConstruction, true, Guid.NewGuid());
        var contentTree = """{"chapters": [{"id": "1", "title": "Memoria Descriptiva"}]}""";

        _repositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new UpdateProjectTreeCommand(projectId, contentTree);
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

        var command = new UpdateProjectTreeCommand(projectId, "{}");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("NotFound");
        result.Error.Description.Should().Contain(projectId.ToString());
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenProjectExists()
    {
        var projectId = Guid.NewGuid();
        var project = Project.Create("Test", InterventionType.Reform, false, Guid.NewGuid());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new UpdateProjectTreeCommand(projectId, """{"test": true}""");
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

        var command = new UpdateProjectTreeCommand(Guid.NewGuid(), "{}");
        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateContentTree_OnProject()
    {
        var projectId = Guid.NewGuid();
        var project = Project.Create("Test", InterventionType.Extension, true, Guid.NewGuid());
        var contentTree = """{"chapters": [{"id": "md", "title": "Memoria Descriptiva", "sections": []}]}""";

        _repositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var command = new UpdateProjectTreeCommand(projectId, contentTree);
        await _handler.Handle(command, CancellationToken.None);

        project.ContentTreeJson.Should().Be(contentTree);
    }
}
