using Edificia.Application.Interfaces;
using Edificia.Application.Templates.Commands.SetTemplateDefault;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Templates.Commands;

public class SetTemplateDefaultHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock;
    private readonly SetTemplateDefaultHandler _handler;

    public SetTemplateDefaultHandlerTests()
    {
        _templateRepositoryMock = new Mock<ITemplateRepository>();

        _handler = new SetTemplateDefaultHandler(
            _templateRepositoryMock.Object,
            Mock.Of<ILogger<SetTemplateDefaultHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTemplateDoesNotExist()
    {
        var command = new SetTemplateDefaultCommand(Guid.NewGuid(), true);

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppTemplate?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Template.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReplaceCurrentDefault_WhenSettingNewDefault()
    {
        var currentDefault = AppTemplate.Create(
            "Plantilla A",
            null,
            "MemoriaTecnica",
            "templates/memoria/a.dotx",
            "a.dotx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            100,
            Guid.NewGuid());
        currentDefault.SetAvailable(true);
        currentDefault.MarkAsDefault();

        var targetTemplate = AppTemplate.Create(
            "Plantilla B",
            null,
            "MemoriaTecnica",
            "templates/memoria/b.dotx",
            "b.dotx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            100,
            Guid.NewGuid());
        targetTemplate.SetAvailable(true);

        var command = new SetTemplateDefaultCommand(Guid.NewGuid(), true);
        SetEntityId(targetTemplate, command.TemplateId);

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetTemplate);

        _templateRepositoryMock
            .Setup(x => x.GetDefaultByTypeAsync("MemoriaTecnica", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentDefault);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        currentDefault.IsDefault.Should().BeFalse();
        targetTemplate.IsDefault.Should().BeTrue();
        _templateRepositoryMock.Verify(x => x.Update(currentDefault), Times.Once);
        _templateRepositoryMock.Verify(x => x.Update(targetTemplate), Times.Once);
        _templateRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldClearDefault_WhenRequestIsFalse()
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

        var command = new SetTemplateDefaultCommand(Guid.NewGuid(), false);
        SetEntityId(template, command.TemplateId);

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        template.IsDefault.Should().BeFalse();
        _templateRepositoryMock.Verify(x => x.Update(template), Times.Once);
        _templateRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SetEntityId(AppTemplate template, Guid id)
    {
        var property = typeof(Edificia.Domain.Primitives.Entity)
            .GetProperty("Id")!;
        property.SetValue(template, id);
    }
}