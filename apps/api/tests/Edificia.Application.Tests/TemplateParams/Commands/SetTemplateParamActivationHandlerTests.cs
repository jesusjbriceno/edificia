using Edificia.Application.Interfaces;
using Edificia.Application.TemplateParams.Commands.SetTemplateParamActivation;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.TemplateParams.Commands;

public class SetTemplateParamActivationHandlerTests
{
    private readonly Mock<ITemplateParamRepository> _templateParamRepositoryMock;
    private readonly SetTemplateParamActivationHandler _handler;

    public SetTemplateParamActivationHandlerTests()
    {
        _templateParamRepositoryMock = new Mock<ITemplateParamRepository>();

        _handler = new SetTemplateParamActivationHandler(
            _templateParamRepositoryMock.Object,
            Mock.Of<ILogger<SetTemplateParamActivationHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenParamDoesNotExist()
    {
        var command = new SetTemplateParamActivationCommand(Guid.NewGuid(), true);

        _templateParamRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateParamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TemplateParam?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("TemplateParam.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldSetActivation_WhenParamExists()
    {
        var templateParam = TemplateParam.Create(
            key: "PROJECT_TITLE",
            displayName: "Titulo del proyecto",
            sourceCode: "PROJECT_TITLE",
            formatter: null,
            isActive: false);

        var command = new SetTemplateParamActivationCommand(Guid.NewGuid(), true);
        SetEntityId(templateParam, command.TemplateParamId);

        _templateParamRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateParamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(templateParam);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        templateParam.IsActive.Should().BeTrue();
        _templateParamRepositoryMock.Verify(x => x.Update(templateParam), Times.Once);
        _templateParamRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SetEntityId(TemplateParam templateParam, Guid id)
    {
        var property = typeof(Edificia.Domain.Primitives.Entity)
            .GetProperty("Id")!;
        property.SetValue(templateParam, id);
    }
}
