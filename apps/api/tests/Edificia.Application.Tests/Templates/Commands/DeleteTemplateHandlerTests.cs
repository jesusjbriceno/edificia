using Edificia.Application.Interfaces;
using Edificia.Application.Templates.Commands.DeleteTemplate;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Templates.Commands;

public class DeleteTemplateHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly DeleteTemplateHandler _handler;

    public DeleteTemplateHandlerTests()
    {
        _templateRepositoryMock = new Mock<ITemplateRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();

        _handler = new DeleteTemplateHandler(
            _templateRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            Mock.Of<ILogger<DeleteTemplateHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTemplateDoesNotExist()
    {
        var command = new DeleteTemplateCommand(Guid.NewGuid());

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppTemplate?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Template.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenTemplateIsDefault()
    {
        var template = AppTemplate.Create(
            "Plantilla",
            null,
            "MemoriaTecnica",
            "templates/memoria/v1.dotx",
            "v1.dotx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            100,
            Guid.NewGuid());
        template.SetAvailable(true);
        template.MarkAsDefault();

        var command = new DeleteTemplateCommand(Guid.NewGuid());
        SetEntityId(template, command.TemplateId);

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Template.CannotDeleteDefault");
        _fileStorageServiceMock.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteTemplate_WhenTemplateIsNotDefault()
    {
        var template = AppTemplate.Create(
            "Plantilla",
            null,
            "MemoriaTecnica",
            "templates/memoria/v1.dotx",
            "v1.dotx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            100,
            Guid.NewGuid());

        var command = new DeleteTemplateCommand(Guid.NewGuid());
        SetEntityId(template, command.TemplateId);

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _fileStorageServiceMock
            .Setup(x => x.DeleteFileAsync(template.StoragePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _templateRepositoryMock.Verify(x => x.Remove(template), Times.Once);
        _templateRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SetEntityId(AppTemplate template, Guid id)
    {
        var property = typeof(Edificia.Domain.Primitives.Entity)
            .GetProperty("Id")!;
        property.SetValue(template, id);
    }
}