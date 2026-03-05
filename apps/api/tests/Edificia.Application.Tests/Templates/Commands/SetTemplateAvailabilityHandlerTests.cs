using Edificia.Application.Interfaces;
using Edificia.Application.Templates.Commands.SetTemplateAvailability;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Templates.Commands;

public class SetTemplateAvailabilityHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock;
    private readonly SetTemplateAvailabilityHandler _handler;

    public SetTemplateAvailabilityHandlerTests()
    {
        _templateRepositoryMock = new Mock<ITemplateRepository>();

        _handler = new SetTemplateAvailabilityHandler(
            _templateRepositoryMock.Object,
            Mock.Of<ILogger<SetTemplateAvailabilityHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTemplateDoesNotExist()
    {
        var command = new SetTemplateAvailabilityCommand(Guid.NewGuid(), true);

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppTemplate?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Template.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldSetAvailability_WhenTemplateExists()
    {
        var template = AppTemplate.Create(
            "Plantilla",
            null,
            "MemoriaTecnica",
            "templates/memoria/v1.dotx",
            "v1.dotx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            123,
            Guid.NewGuid());

        var command = new SetTemplateAvailabilityCommand(Guid.NewGuid(), true);
        SetEntityId(template, command.TemplateId);

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        template.IsAvailable.Should().BeTrue();
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