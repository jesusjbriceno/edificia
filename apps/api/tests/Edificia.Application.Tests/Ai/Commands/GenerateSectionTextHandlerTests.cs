using Edificia.Application.Ai.Commands.GenerateSectionText;
using Edificia.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace Edificia.Application.Tests.Ai.Commands;

public class GenerateSectionTextHandlerTests
{
    private readonly Mock<IAiService> _aiServiceMock;
    private readonly GenerateSectionTextHandler _handler;

    public GenerateSectionTextHandlerTests()
    {
        _aiServiceMock = new Mock<IAiService>();
        _handler = new GenerateSectionTextHandler(_aiServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithGeneratedText()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(), "md-1", "Describe los agentes", "Vivienda unifamiliar");

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("<p>Los agentes intervinientes en el proyecto son...</p>");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.GeneratedText.Should().Contain("agentes");
    }

    [Fact]
    public async Task Handle_ShouldReturnResponse_WithCorrectIds()
    {
        var projectId = Guid.NewGuid();
        var command = new GenerateSectionTextCommand(
            projectId, "md-1", "Describe agentes", null);

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Texto generado");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProjectId.Should().Be(projectId);
        result.Value.SectionId.Should().Be("md-1");
    }

    [Fact]
    public async Task Handle_ShouldCallAiService_WithFormattedPrompt()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(), "md-1", "Describe los agentes", "Vivienda unifamiliar en Madrid");

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Respuesta");

        await _handler.Handle(command, CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(
                It.Is<string>(p => p.Contains("Describe los agentes") && p.Contains("Vivienda unifamiliar")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallAiService_WithOnlyPrompt_WhenContextIsNull()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(), "md-1", "Describe los agentes", null);

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Respuesta");

        await _handler.Handle(command, CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(
                It.Is<string>(p => p.Contains("Describe los agentes")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiServiceThrows()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(), "md-1", "Prompt", null);

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Failure");
        result.Error.Description.Should().Contain("IA");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiServiceReturnsEmpty()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(), "md-1", "Prompt", null);

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Failure");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiServiceReturnsNull()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(), "md-1", "Prompt", null);

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToAiService()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(), "md-1", "Prompt", null);
        var cts = new CancellationTokenSource();

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<string>(), cts.Token))
            .ReturnsAsync("Respuesta");

        await _handler.Handle(command, cts.Token);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(It.IsAny<string>(), cts.Token),
            Times.Once);
    }
}
