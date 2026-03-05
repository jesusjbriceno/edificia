using Edificia.Application.Interfaces;
using Edificia.Application.Templates.Commands.UpdateTemplateMetadata;
using Edificia.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Templates.Commands;

public class UpdateTemplateMetadataHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock;
    private readonly UpdateTemplateMetadataHandler _handler;

    public UpdateTemplateMetadataHandlerTests()
    {
        _templateRepositoryMock = new Mock<ITemplateRepository>();

        _handler = new UpdateTemplateMetadataHandler(
            _templateRepositoryMock.Object,
            Mock.Of<ILogger<UpdateTemplateMetadataHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTemplateDoesNotExist()
    {
        var command = new UpdateTemplateMetadataCommand(Guid.NewGuid(), "Nombre", "Descripción");

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppTemplate?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Template.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldUpdateTemplateMetadata_WhenTemplateExists()
    {
        var template = AppTemplate.Create(
            "Plantilla original",
            "Descripción original",
            "MemoriaTecnica",
            "templates/memoria/v1.dotx",
            "v1.dotx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            123,
            Guid.NewGuid());

        var command = new UpdateTemplateMetadataCommand(Guid.NewGuid(), "Plantilla nueva", "Descripción nueva");

        SetEntityId(template, command.TemplateId);

        _templateRepositoryMock
            .Setup(x => x.GetByIdAsync(command.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        template.Name.Should().Be("Plantilla nueva");
        template.Description.Should().Be("Descripción nueva");
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