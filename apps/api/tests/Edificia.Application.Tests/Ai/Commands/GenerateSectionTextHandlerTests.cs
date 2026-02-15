using Edificia.Application.Ai.Commands.GenerateSectionText;
using Edificia.Application.Ai.Services;
using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Edificia.Application.Tests.Ai.Commands;

public class GenerateSectionTextHandlerTests
{
    private readonly Mock<IAiService> _aiServiceMock;
    private readonly Mock<IProjectRepository> _repositoryMock;
    private readonly Mock<IPromptTemplateService> _templateServiceMock;
    private readonly GenerateSectionTextHandler _handler;

    public GenerateSectionTextHandlerTests()
    {
        _aiServiceMock = new Mock<IAiService>();
        _repositoryMock = new Mock<IProjectRepository>();
        _templateServiceMock = new Mock<IPromptTemplateService>();
        _handler = new GenerateSectionTextHandler(
            _aiServiceMock.Object,
            _repositoryMock.Object,
            _templateServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithGeneratedText()
    {
        var project = CreateProject();
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Describe los agentes", "Vivienda unifamiliar");

        SetupProjectFound(project);
        SetupTemplateService("Prompt formateado completo");
        SetupAiResponse("<p>Los agentes intervinientes en el proyecto son...</p>");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.GeneratedText.Should().Contain("agentes");
    }

    [Fact]
    public async Task Handle_ShouldReturnResponse_WithCorrectIds()
    {
        var project = CreateProject();
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Describe agentes", null);

        SetupProjectFound(project);
        SetupTemplateService("Prompt formateado");
        SetupAiResponse("Texto generado");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProjectId.Should().Be(project.Id);
        result.Value.SectionId.Should().Be("md-1");
    }

    [Fact]
    public async Task Handle_ShouldLoadProject_FromRepository()
    {
        var project = CreateProject();
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Describe agentes", null);

        SetupProjectFound(project);
        SetupTemplateService("Prompt");
        SetupAiResponse("Respuesta");

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProjectDoesNotExist()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(), "md-1", "Prompt", null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(command.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("NotFound");
    }

    [Fact]
    public async Task Handle_ShouldBuildPrompt_UsingTemplateService()
    {
        var project = CreateProject();
        var command = new GenerateSectionTextCommand(
            project.Id, "DB-HE-01", "Redacta ahorro energético", "Contenido previo");

        SetupProjectFound(project);
        SetupTemplateService("Prompt enriquecido con contexto");
        SetupAiResponse("Respuesta IA");

        await _handler.Handle(command, CancellationToken.None);

        _templateServiceMock.Verify(
            t => t.BuildSectionPrompt(It.Is<SectionPromptContext>(c =>
                c.SectionId == "DB-HE-01" &&
                c.UserPrompt == "Redacta ahorro energético" &&
                c.ExistingContent == "Contenido previo" &&
                c.ProjectTitle == project.Title &&
                c.InterventionType == project.InterventionType &&
                c.IsLoeRequired == project.IsLoeRequired)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSendTemplateResult_ToAiService()
    {
        var project = CreateProject();
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt usuario", null);

        SetupProjectFound(project);
        SetupTemplateService("Prompt enriquecido por template service");
        SetupAiResponse("Respuesta");

        await _handler.Handle(command, CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(
                "Prompt enriquecido por template service",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiServiceThrows()
    {
        var project = CreateProject();
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt", null);

        SetupProjectFound(project);
        SetupTemplateService("Prompt");

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
        var project = CreateProject();
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt", null);

        SetupProjectFound(project);
        SetupTemplateService("Prompt");
        SetupAiResponse(string.Empty);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Failure");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiServiceReturnsNull()
    {
        var project = CreateProject();
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt", null);

        SetupProjectFound(project);
        SetupTemplateService("Prompt");

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
        var project = CreateProject();
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt", null);
        var cts = new CancellationTokenSource();

        SetupProjectFound(project);
        SetupTemplateService("Prompt");

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(It.IsAny<string>(), cts.Token))
            .ReturnsAsync("Respuesta");

        await _handler.Handle(command, cts.Token);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(It.IsAny<string>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassProjectAddress_ToTemplateContext()
    {
        var project = CreateProject(address: "Calle Serrano 10, Madrid");
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt", null);

        SetupProjectFound(project);
        SetupTemplateService("Prompt");
        SetupAiResponse("Respuesta");

        await _handler.Handle(command, CancellationToken.None);

        _templateServiceMock.Verify(
            t => t.BuildSectionPrompt(It.Is<SectionPromptContext>(c =>
                c.Address == "Calle Serrano 10, Madrid")),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassLocalRegulations_ToTemplateContext()
    {
        var project = CreateProject(localRegulations: "PGOU Madrid 2024");
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt", null);

        SetupProjectFound(project);
        SetupTemplateService("Prompt");
        SetupAiResponse("Respuesta");

        await _handler.Handle(command, CancellationToken.None);

        _templateServiceMock.Verify(
            t => t.BuildSectionPrompt(It.Is<SectionPromptContext>(c =>
                c.LocalRegulations == "PGOU Madrid 2024")),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCallAiService_WhenProjectNotFound()
    {
        var command = new GenerateSectionTextCommand(
            Guid.NewGuid(), "md-1", "Prompt", null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(command.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        await _handler.Handle(command, CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── Helpers ──

    private static Project CreateProject(
        InterventionType interventionType = InterventionType.NewConstruction,
        bool isLoeRequired = true,
        string? address = null,
        string? localRegulations = null)
    {
        return Project.Create(
            title: "Proyecto Test",
            interventionType: interventionType,
            isLoeRequired: isLoeRequired,
            description: "Descripción de prueba",
            address: address,
            localRegulations: localRegulations);
    }

    private void SetupProjectFound(Project project)
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
    }

    private void SetupTemplateService(string formattedPrompt)
    {
        _templateServiceMock
            .Setup(t => t.BuildSectionPrompt(It.IsAny<SectionPromptContext>()))
            .Returns(formattedPrompt);
    }

    private void SetupAiResponse(string response)
    {
        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }
}
