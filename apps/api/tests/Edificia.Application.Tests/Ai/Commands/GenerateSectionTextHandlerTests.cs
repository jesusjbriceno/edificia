using Edificia.Application.Ai.Commands.GenerateSectionText;
using Edificia.Application.Ai.Dtos;
using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Ai.Commands;

public class GenerateSectionTextHandlerTests
{
    private readonly Mock<IAiService> _aiServiceMock;
    private readonly Mock<IProjectRepository> _repositoryMock;
    private readonly GenerateSectionTextHandler _handler;

    public GenerateSectionTextHandlerTests()
    {
        _aiServiceMock = new Mock<IAiService>();
        _repositoryMock = new Mock<IProjectRepository>();
        _handler = new GenerateSectionTextHandler(
            _aiServiceMock.Object,
            _repositoryMock.Object,
            Mock.Of<ILogger<GenerateSectionTextHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithGeneratedText()
    {
        var project = CreateProject();
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Describe los agentes", "Vivienda unifamiliar");

        SetupProjectFound(project);
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
    public async Task Handle_ShouldBuildAiRequest_WithProjectMetadata()
    {
        var project = CreateProject(
            address: "Calle Serrano 10, Madrid",
            localRegulations: "PGOU Madrid 2024");
        var command = new GenerateSectionTextCommand(
            project.Id, "DB-HE-01", "Redacta ahorro energético", "Contenido previo");

        SetupProjectFound(project);
        SetupAiResponse("Respuesta IA");

        await _handler.Handle(command, CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(
                It.Is<AiGenerationRequest>(r =>
                    r.SectionCode == "DB-HE-01" &&
                    r.ProjectType == "NewConstruction" &&
                    r.UserInstructions == "Redacta ahorro energético" &&
                    r.TechnicalContext != null &&
                    r.TechnicalContext.ProjectTitle == project.Title &&
                    r.TechnicalContext.InterventionType == "Obra Nueva" &&
                    r.TechnicalContext.IsLoeRequired == true &&
                    r.TechnicalContext.Address == "Calle Serrano 10, Madrid" &&
                    r.TechnicalContext.LocalRegulations == "PGOU Madrid 2024" &&
                    r.TechnicalContext.ExistingContent == "Contenido previo"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSendAiRequest_ToAiService()
    {
        var project = CreateProject();
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt usuario", null);

        SetupProjectFound(project);
        SetupAiResponse("Respuesta");

        await _handler.Handle(command, CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(
                It.Is<AiGenerationRequest>(r => r.SectionCode == "md-1"),
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

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<AiGenerationRequest>(), It.IsAny<CancellationToken>()))
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

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<AiGenerationRequest>(), It.IsAny<CancellationToken>()))
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

        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(It.IsAny<AiGenerationRequest>(), cts.Token))
            .ReturnsAsync("Respuesta");

        await _handler.Handle(command, cts.Token);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(It.IsAny<AiGenerationRequest>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldIncludeAddress_InTechnicalContext()
    {
        var project = CreateProject(address: "Calle Serrano 10, Madrid");
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt", null);

        SetupProjectFound(project);
        SetupAiResponse("Respuesta");

        await _handler.Handle(command, CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(
                It.Is<AiGenerationRequest>(r =>
                    r.TechnicalContext != null &&
                    r.TechnicalContext.Address == "Calle Serrano 10, Madrid"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldIncludeLocalRegulations_InTechnicalContext()
    {
        var project = CreateProject(localRegulations: "PGOU Madrid 2024");
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt", null);

        SetupProjectFound(project);
        SetupAiResponse("Respuesta");

        await _handler.Handle(command, CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(
                It.Is<AiGenerationRequest>(r =>
                    r.TechnicalContext != null &&
                    r.TechnicalContext.LocalRegulations == "PGOU Madrid 2024"),
                It.IsAny<CancellationToken>()),
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
            s => s.GenerateTextAsync(It.IsAny<AiGenerationRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMapReformProject_ToCorrectProjectType()
    {
        var project = CreateProject(interventionType: InterventionType.Reform, isLoeRequired: false);
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt", null);

        SetupProjectFound(project);
        SetupAiResponse("Respuesta");

        await _handler.Handle(command, CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(
                It.Is<AiGenerationRequest>(r =>
                    r.ProjectType == "Reform" &&
                    r.TechnicalContext != null &&
                    r.TechnicalContext.InterventionType == "Reforma" &&
                    r.TechnicalContext.IsLoeRequired == false),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapExtensionProject_ToCorrectProjectType()
    {
        var project = CreateProject(interventionType: InterventionType.Extension);
        var command = new GenerateSectionTextCommand(
            project.Id, "md-1", "Prompt", null);

        SetupProjectFound(project);
        SetupAiResponse("Respuesta");

        await _handler.Handle(command, CancellationToken.None);

        _aiServiceMock.Verify(
            s => s.GenerateTextAsync(
                It.Is<AiGenerationRequest>(r =>
                    r.ProjectType == "Extension" &&
                    r.TechnicalContext != null &&
                    r.TechnicalContext.InterventionType == "Ampliación"),
                It.IsAny<CancellationToken>()),
            Times.Once);
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

    private void SetupAiResponse(string response)
    {
        _aiServiceMock
            .Setup(s => s.GenerateTextAsync(
                It.IsAny<AiGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }
}
