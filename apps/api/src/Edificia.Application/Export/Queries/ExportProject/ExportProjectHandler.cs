using System.Text.RegularExpressions;
using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Export.Queries.ExportProject;

/// <summary>
/// Handles the export of a project's content tree to a .docx document.
/// </summary>
public sealed partial class ExportProjectHandler : IRequestHandler<ExportProjectQuery, Result<ExportDocumentResponse>>
{
    private const string DocxContentType =
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    private const string DefaultTemplateType = "MemoriaTecnica";
    private static readonly TimeSpan TemplateCacheDuration = TimeSpan.FromMinutes(15);

    private readonly IProjectRepository _repository;
    private readonly ITemplateRepository _templateRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentExportService _exportService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<ExportProjectHandler> _logger;

    public ExportProjectHandler(
        IProjectRepository repository,
        ITemplateRepository templateRepository,
        IFileStorageService fileStorageService,
        IDocumentExportService exportService,
        IMemoryCache memoryCache,
        ILogger<ExportProjectHandler> logger)
    {
        _repository = repository;
        _templateRepository = templateRepository;
        _fileStorageService = fileStorageService;
        _exportService = exportService;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<Result<ExportDocumentResponse>> Handle(
        ExportProjectQuery request,
        CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
        {
            _logger.LogWarning("Project {ProjectId} not found for export", request.ProjectId);
            return Result.NotFound<ExportDocumentResponse>(
                Error.NotFound("Project", $"No se encontró el proyecto con ID {request.ProjectId}."));
        }

        if (string.IsNullOrWhiteSpace(project.ContentTreeJson))
        {
            _logger.LogWarning("Project {ProjectId} has no content tree for export", request.ProjectId);
            return Result.Failure<ExportDocumentResponse>(
                Error.Failure("Export.NoContent",
                    "El proyecto no tiene contenido para exportar. Añada contenido antes de exportar."));
        }

        var exportData = ExportDocumentData.FromProject(project);
        var templateResolution = await ResolveTemplateAsync(request.TemplateId, cancellationToken);
        if (templateResolution.IsFailure)
        {
            return Result.Failure<ExportDocumentResponse>(templateResolution.Error);
        }

        var templateBytes = await TryGetTemplateBytesAsync(templateResolution.Value, cancellationToken);

        try
        {
            byte[] fileContent;
            if (templateBytes is { Length: > 0 })
            {
                try
                {
                    fileContent = await _exportService.ExportToDocxWithTemplateAsync(
                        exportData,
                        templateBytes,
                        cancellationToken);
                }
                catch (Exception templateEx)
                {
                    _logger.LogWarning(templateEx,
                        "Template export failed for project {ProjectId}. Falling back to legacy export.",
                        request.ProjectId);
                    fileContent = await _exportService.ExportToDocxAsync(exportData, cancellationToken);
                }
            }
            else
            {
                fileContent = await _exportService.ExportToDocxAsync(exportData, cancellationToken);
            }

            if (fileContent is null || fileContent.Length == 0)
            {
                _logger.LogError("Export service returned empty result for project {ProjectId}", request.ProjectId);
                return Result.Failure<ExportDocumentResponse>(
                    Error.Failure("Export.EmptyResult",
                        "No se pudo exportar el documento. El servicio de exportación no generó contenido."));
            }

            var fileName = ResolveOutputFileName(request.OutputFileName, project.Title);

            _logger.LogInformation("Successfully exported project {ProjectId} ({FileName}, {Size} bytes)",
                request.ProjectId, fileName, fileContent.Length);

            return Result.Success(new ExportDocumentResponse(fileContent, fileName, DocxContentType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting project {ProjectId}", request.ProjectId);
            return Result.Failure<ExportDocumentResponse>(
                Error.Failure("Export.Error",
                    $"Error al exportar el documento: {ex.Message}"));
        }
    }

    private async Task<Result<AppTemplate?>> ResolveTemplateAsync(Guid? preferredTemplateId, CancellationToken cancellationToken)
    {
        if (preferredTemplateId.HasValue)
        {
            var preferredTemplate = await _templateRepository.GetByIdAsync(preferredTemplateId.Value, cancellationToken);
            if (preferredTemplate is not null)
            {
                if (!string.Equals(preferredTemplate.TemplateType, DefaultTemplateType, StringComparison.OrdinalIgnoreCase))
                {
                    return Result.Failure<AppTemplate?>(
                        Error.Validation(
                            "Export.TemplateTypeMismatch",
                            "La plantilla seleccionada no corresponde al tipo documental de exportación."));
                }

                if (preferredTemplate.IsAvailable)
                {
                    return Result.Success<AppTemplate?>(preferredTemplate);
                }

                _logger.LogInformation(
                    "Preferred template {TemplateId} is not available. Falling back to default template resolution.",
                    preferredTemplate.Id);
            }
            else
            {
                _logger.LogInformation(
                    "Preferred template {TemplateId} was not found. Falling back to default template resolution.",
                    preferredTemplateId.Value);
            }
        }

        var defaultTemplate = await _templateRepository.GetDefaultByTypeAsync(DefaultTemplateType, cancellationToken)
            ?? await _templateRepository.GetActiveByTypeAsync(DefaultTemplateType, cancellationToken);

        return Result.Success<AppTemplate?>(defaultTemplate);
    }

    private async Task<byte[]?> TryGetTemplateBytesAsync(AppTemplate? selectedTemplate, CancellationToken cancellationToken)
    {
        if (selectedTemplate is null)
        {
            return null;
        }

        var cacheKey = $"template-export:{selectedTemplate.TemplateType}:{selectedTemplate.Id}:{selectedTemplate.Version}";
        if (_memoryCache.TryGetValue<byte[]>(cacheKey, out var cachedBytes) && cachedBytes is { Length: > 0 })
        {
            return cachedBytes;
        }

        try
        {
            var templateBytes = await _fileStorageService.GetFileAsync(selectedTemplate.StoragePath, cancellationToken);
            if (templateBytes.Length == 0)
            {
                return null;
            }

            _memoryCache.Set(cacheKey, templateBytes, TemplateCacheDuration);
            return templateBytes;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Unable to load selected template from storage for template {TemplateId}. Falling back to legacy export.",
                selectedTemplate.Id);
            return null;
        }
    }

    private static string ResolveOutputFileName(string? requestedOutputFileName, string projectTitle)
    {
        if (!string.IsNullOrWhiteSpace(requestedOutputFileName))
        {
            var normalized = NormalizeRequestedFileName(requestedOutputFileName);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                return normalized;
            }
        }

        return SanitizeFileName(projectTitle) + ".docx";
    }

    private static string? NormalizeRequestedFileName(string requestedOutputFileName)
    {
        var trimmed = requestedOutputFileName.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        var withoutExtension = trimmed.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)
            ? trimmed[..^5]
            : trimmed;

        var sanitizedBaseName = SanitizeFileName(withoutExtension);
        if (string.IsNullOrWhiteSpace(sanitizedBaseName))
        {
            return null;
        }

        return sanitizedBaseName + ".docx";
    }

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
