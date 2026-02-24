using Edificia.Application.Interfaces;
using Edificia.Application.Templates.Commands.CreateTemplate;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Templates.Commands;

public class CreateTemplateHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<CreateTemplateHandler>> _loggerMock;
    private readonly CreateTemplateHandler _handler;

    public CreateTemplateHandlerTests()
    {
        _templateRepositoryMock = new Mock<ITemplateRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<CreateTemplateHandler>>();

        _handler = new CreateTemplateHandler(
            _templateRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTemplateIsCreated()
    {
        var command = BuildValidCommand();

        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(
                It.IsAny<Stream>(),
                command.OriginalFileName,
                command.TemplateType,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("templates/memoria/v1.dotx");

        _templateRepositoryMock
            .Setup(x => x.GetActiveByTypeAsync(command.TemplateType, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppTemplate?)null);

        _templateRepositoryMock
            .Setup(x => x.CountByTypeAsync(command.TemplateType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _templateRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AppTemplate>(), It.IsAny<CancellationToken>()), Times.Once);
        _templateRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenStorageThrows()
    {
        var command = BuildValidCommand();

        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Storage error"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Template.StorageFailed");

        _templateRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AppTemplate>(), It.IsAny<CancellationToken>()), Times.Never);
        _templateRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateInactiveNextVersion_WhenActiveTemplateExists()
    {
        var command = BuildValidCommand();
        AppTemplate? savedTemplate = null;

        var activeTemplate = AppTemplate.Create(
            "Plantilla activa",
            null,
            command.TemplateType,
            "templates/memoria/v1.dotx",
            "v1.dotx",
            command.MimeType,
            20,
            Guid.NewGuid());
        activeTemplate.Activate();

        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(
                It.IsAny<Stream>(),
                command.OriginalFileName,
                command.TemplateType,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("templates/memoria/v2.dotx");

        _templateRepositoryMock
            .Setup(x => x.GetActiveByTypeAsync(command.TemplateType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeTemplate);

        _templateRepositoryMock
            .Setup(x => x.CountByTypeAsync(command.TemplateType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _templateRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AppTemplate>(), It.IsAny<CancellationToken>()))
            .Callback<AppTemplate, CancellationToken>((template, _) => savedTemplate = template)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        savedTemplate.Should().NotBeNull();
        savedTemplate!.Version.Should().Be(2);
        savedTemplate.IsActive.Should().BeFalse();
    }

    private static CreateTemplateCommand BuildValidCommand()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };

        return new CreateTemplateCommand(
            Name: "Plantilla Base",
            TemplateType: "MemoriaTecnica",
            Description: "Descripción",
            OriginalFileName: "plantilla.dotx",
            MimeType: "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            FileSizeBytes: bytes.Length,
            FileContent: bytes,
            CreatedByUserId: Guid.NewGuid());
    }
}
