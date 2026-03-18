using Edificia.Application.Interfaces;
using Edificia.Application.Templates.Commands.ToggleTemplateStatus;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Templates.Commands;

public class ToggleTemplateStatusHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock;
    private readonly Mock<ILogger<ToggleTemplateStatusHandler>> _loggerMock;
    private readonly ToggleTemplateStatusHandler _handler;

    public ToggleTemplateStatusHandlerTests()
    {
        _templateRepositoryMock = new Mock<ITemplateRepository>();
        _loggerMock = new Mock<ILogger<ToggleTemplateStatusHandler>>();

        _handler = new ToggleTemplateStatusHandler(_templateRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTemplateDoesNotExist()
    {
        var command = new ToggleTemplateStatusCommand(Guid.NewGuid(), true);

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppTemplate?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Template.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldSetAvailabilityWithoutAffectingOtherTemplates()
    {
        var templateId = Guid.NewGuid();

        var targetTemplate = AppTemplate.Create(
            "Nueva plantilla",
            null,
            "MemoriaTecnica",
            "templates/memoria/v2.dotx",
            "v2.dotx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            123,
            Guid.NewGuid());

        SetEntityId(targetTemplate, templateId);

        var command = new ToggleTemplateStatusCommand(templateId, true);

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetTemplate);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        targetTemplate.IsAvailable.Should().BeTrue();

        _templateRepositoryMock.Verify(x => x.Update(targetTemplate), Times.Once);
        _templateRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SetEntityId(AppTemplate template, Guid id)
    {
        var property = typeof(Edificia.Domain.Primitives.Entity)
            .GetProperty("Id")!;
        property.SetValue(template, id);
    }
}
