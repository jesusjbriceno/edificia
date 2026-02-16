using System.Text.RegularExpressions;
using Edificia.Application.Interfaces;
using Edificia.Domain.Enums;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Export.Queries.ExportProject;

/// <summary>
/// Handles the export of a project's content tree to a .docx document.
/// </summary>
public sealed partial class ExportProjectHandler : IRequestHandler<ExportProjectQuery, Result<ExportDocumentResponse>>
{
    private const string DocxContentType =
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    private readonly IProjectRepository _repository;
    private readonly IDocumentExportService _exportService;
    private readonly ILogger<ExportProjectHandler>? _logger;

    /// <summary>Constructor for unit testing (without ILogger).</summary>
    public ExportProjectHandler(
        IProjectRepository repository,
        IDocumentExportService exportService)
    {
        _repository = repository;
        _exportService = exportService;
    }

    /// <summary>Constructor for DI (with ILogger).</summary>
    public ExportProjectHandler(
        IProjectRepository repository,
        IDocumentExportService exportService,
        ILogger<ExportProjectHandler> logger) : this(repository, exportService)
    {
        _logger = logger;
    }

    public async Task<Result<ExportDocumentResponse>> Handle(
        ExportProjectQuery request,
        CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
        {
            _logger?.LogWarning("Project {ProjectId} not found for export", request.ProjectId);
            return Result.NotFound<ExportDocumentResponse>(
                Error.NotFound("Project", $"No se encontró el proyecto con ID {request.ProjectId}."));
        }

        if (string.IsNullOrWhiteSpace(project.ContentTreeJson))
        {
            _logger?.LogWarning("Project {ProjectId} has no content tree for export", request.ProjectId);
            return Result.Failure<ExportDocumentResponse>(
                Error.Failure("Export.NoContent",
                    "El proyecto no tiene contenido para exportar. Añada contenido antes de exportar."));
        }

        var exportData = new ExportDocumentData(
            Title: project.Title,
            InterventionType: FormatInterventionType(project.InterventionType),
            IsLoeRequired: project.IsLoeRequired,
            ContentTreeJson: project.ContentTreeJson,
            Address: project.Address);

        try
        {
            var fileContent = await _exportService.ExportToDocxAsync(exportData, cancellationToken);

            if (fileContent is null || fileContent.Length == 0)
            {
                _logger?.LogError("Export service returned empty result for project {ProjectId}", request.ProjectId);
                return Result.Failure<ExportDocumentResponse>(
                    Error.Failure("Export.EmptyResult",
                        "No se pudo exportar el documento. El servicio de exportación no generó contenido."));
            }

            var fileName = SanitizeFileName(project.Title) + ".docx";

            _logger?.LogInformation("Successfully exported project {ProjectId} ({FileName}, {Size} bytes)",
                request.ProjectId, fileName, fileContent.Length);

            return Result.Success(new ExportDocumentResponse(fileContent, fileName, DocxContentType));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error exporting project {ProjectId}", request.ProjectId);
            return Result.Failure<ExportDocumentResponse>(
                Error.Failure("Export.Error",
                    $"Error al exportar el documento: {ex.Message}"));
        }
    }

    private static string FormatInterventionType(InterventionType type) => type switch
    {
        InterventionType.NewConstruction => "Obra Nueva",
        InterventionType.Reform => "Reforma",
        InterventionType.Extension => "Ampliación",
        _ => type.ToString()
    };

    private static string SanitizeFileName(string title)
    {
        // Remove characters that are invalid in file names
        var sanitized = InvalidFileCharsRegex().Replace(title, "_");
        // Collapse multiple underscores
        sanitized = MultipleUnderscoreRegex().Replace(sanitized, "_");
        // Trim and limit length
        sanitized = sanitized.Trim('_');
        return sanitized.Length > 100 ? sanitized[..100] : sanitized;
    }

    [GeneratedRegex(@"[<>:""/\\|?*]")]
    private static partial Regex InvalidFileCharsRegex();

    [GeneratedRegex(@"_{2,}")]
    private static partial Regex MultipleUnderscoreRegex();
}
