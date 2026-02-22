using Edificia.Application.Export.Queries.ExportProject;
using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Edificia.Application.Tests.Export.Queries;

public class ExportProjectHandlerTests
{
    private readonly Mock<IProjectRepository> _repositoryMock;
    private readonly Mock<IDocumentExportService> _exportServiceMock;
    private readonly ExportProjectHandler _handler;

    public ExportProjectHandlerTests()
    {
        _repositoryMock = new Mock<IProjectRepository>();
        _exportServiceMock = new Mock<IDocumentExportService>();
        _handler = new ExportProjectHandler(
            _repositoryMock.Object,
            _exportServiceMock.Object,
            Mock.Of<ILogger<ExportProjectHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithExportedDocument()
    {
        var project = CreateProjectWithContent();
        var query = new ExportProjectQuery(project.Id);

        SetupProjectFound(project);
        SetupExportService(new byte[] { 0x50, 0x4B }); // PK header (zip/docx)

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FileContent.Should().NotBeEmpty();
        result.Value.FileName.Should().Contain(project.Title);
        result.Value.ContentType.Should().Be(
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProjectDoesNotExist()
    {
        var query = new ExportProjectQuery(Guid.NewGuid());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(query.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenContentTreeIsNull()
    {
        var project = CreateProjectWithoutContent();
        var query = new ExportProjectQuery(project.Id);

        SetupProjectFound(project);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("contenido");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenContentTreeIsEmpty()
    {
        var project = CreateProjectWithContent(contentTree: "");
        var query = new ExportProjectQuery(project.Id);

        SetupProjectFound(project);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCallExportService_WithProjectData()
    {
        var project = CreateProjectWithContent();
        var query = new ExportProjectQuery(project.Id);

        SetupProjectFound(project);
        SetupExportService(new byte[] { 0x50, 0x4B });

        await _handler.Handle(query, CancellationToken.None);

        _exportServiceMock.Verify(
            s => s.ExportToDocxAsync(
                It.Is<ExportDocumentData>(d =>
                    d.Title == project.Title &&
                    d.ContentTreeJson == project.ContentTreeJson),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCallExportService_WhenProjectNotFound()
    {
        var query = new ExportProjectQuery(Guid.NewGuid());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(query.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        await _handler.Handle(query, CancellationToken.None);

        _exportServiceMock.Verify(
            s => s.ExportToDocxAsync(
                It.IsAny<ExportDocumentData>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExportServiceReturnsEmpty()
    {
        var project = CreateProjectWithContent();
        var query = new ExportProjectQuery(project.Id);

        SetupProjectFound(project);
        SetupExportService(Array.Empty<byte>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("exportar");
    }

    [Fact]
    public async Task Handle_ShouldSanitizeFileName_FromProjectTitle()
    {
        var project = CreateProjectWithContent(title: "Edificio \"Gran\" Vía / Test");
        var query = new ExportProjectQuery(project.Id);

        SetupProjectFound(project);
        SetupExportService(new byte[] { 0x50, 0x4B });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().NotContain("\"");
        result.Value.FileName.Should().NotContain("/");
        result.Value.FileName.Should().EndWith(".docx");
    }

    [Fact]
    public async Task Handle_ShouldLoadProject_FromRepository()
    {
        var project = CreateProjectWithContent();
        var query = new ExportProjectQuery(project.Id);

        SetupProjectFound(project);
        SetupExportService(new byte[] { 0x50, 0x4B });

        await _handler.Handle(query, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToExportService()
    {
        var project = CreateProjectWithContent();
        var query = new ExportProjectQuery(project.Id);
        var cts = new CancellationTokenSource();

        SetupProjectFound(project);

        _exportServiceMock
            .Setup(s => s.ExportToDocxAsync(
                It.IsAny<ExportDocumentData>(), cts.Token))
            .ReturnsAsync(new byte[] { 0x50, 0x4B });

        await _handler.Handle(query, cts.Token);

        _exportServiceMock.Verify(
            s => s.ExportToDocxAsync(
                It.IsAny<ExportDocumentData>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldIncludeInterventionType_InExportData()
    {
        var project = CreateProjectWithContent(interventionType: InterventionType.Reform);
        var query = new ExportProjectQuery(project.Id);

        SetupProjectFound(project);
        SetupExportService(new byte[] { 0x50, 0x4B });

        await _handler.Handle(query, CancellationToken.None);

        _exportServiceMock.Verify(
            s => s.ExportToDocxAsync(
                It.Is<ExportDocumentData>(d =>
                    d.InterventionType == "Reforma"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Helpers ──

    private static readonly string SampleContentTree = """
    {
        "chapters": [
            {
                "id": "md-01",
                "title": "Memoria Descriptiva",
                "content": "<p>Descripción del proyecto</p>",
                "sections": [
                    {
                        "id": "md-01-01",
                        "title": "Agentes",
                        "content": "<p>Agentes intervinientes</p>",
                        "sections": []
                    }
                ]
            }
        ]
    }
    """;

    private static Project CreateProjectWithContent(
        string? contentTree = null,
        string title = "Proyecto Test Export",
        InterventionType interventionType = InterventionType.NewConstruction)
    {
        var project = Project.Create(
            title: title,
            interventionType: interventionType,
            isLoeRequired: true,
            createdByUserId: Guid.NewGuid(),
            description: "Descripción de prueba",
            address: "Calle Test 1, Madrid");

        project.UpdateContentTree(contentTree ?? SampleContentTree);
        return project;
    }

    private static Project CreateProjectWithoutContent()
    {
        return Project.Create(
            title: "Proyecto Sin Contenido",
            interventionType: InterventionType.NewConstruction,
            isLoeRequired: true,
            createdByUserId: Guid.NewGuid());
    }

    private void SetupProjectFound(Project project)
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
    }

    private void SetupExportService(byte[] result)
    {
        _exportServiceMock
            .Setup(s => s.ExportToDocxAsync(
                It.IsAny<ExportDocumentData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }
}
