using Edificia.Application.Ai.Dtos;
using Edificia.Application.Interfaces;
using Edificia.Domain.Enums;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Ai.Commands.GenerateSectionText;

/// <summary>
/// Handles GenerateSectionTextCommand by loading the project context,
/// building an AiGenerationRequest with project metadata,
/// and delegating text generation to the AI service (n8n webhook).
/// </summary>
public sealed class GenerateSectionTextHandler
    : IRequestHandler<GenerateSectionTextCommand, Result<GeneratedTextResponse>>
{
    private readonly IAiService _aiService;
    private readonly IProjectRepository _repository;
    private readonly ILogger<GenerateSectionTextHandler> _logger;

    public GenerateSectionTextHandler(
        IAiService aiService,
        IProjectRepository repository,
        ILogger<GenerateSectionTextHandler> logger)
    {
        _aiService = aiService;
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<GeneratedTextResponse>> Handle(
        GenerateSectionTextCommand request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<GeneratedTextResponse>(
                Error.NotFound("Project", $"No se encontró el proyecto con ID {request.ProjectId}."));
        }

        var aiRequest = new AiGenerationRequest(
            SectionCode: request.SectionId,
            ProjectType: FormatProjectType(project.InterventionType),
            TechnicalContext: new TechnicalContext(
                ProjectTitle: project.Title,
                InterventionType: FormatInterventionType(project.InterventionType),
                IsLoeRequired: project.IsLoeRequired,
                Address: project.Address,
                LocalRegulations: project.LocalRegulations,
                ExistingContent: request.Context),
            UserInstructions: request.Prompt);

        try
        {
            _logger.LogInformation(
                "Generating AI text for Project {ProjectId}, Section {SectionId}",
                request.ProjectId, request.SectionId);

            var generatedText = await _aiService.GenerateTextAsync(aiRequest, cancellationToken);

            if (string.IsNullOrWhiteSpace(generatedText))
            {
                _logger.LogWarning(
                    "AI service returned empty response for Project {ProjectId}, Section {SectionId}",
                    request.ProjectId, request.SectionId);

                return Result.Failure<GeneratedTextResponse>(
                    Error.Failure("AiService",
                        "El servicio de IA no devolvió contenido. Inténtelo de nuevo."));
            }

            var response = new GeneratedTextResponse(
                request.ProjectId,
                request.SectionId,
                generatedText);

            return Result.Success(response);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            _logger.LogError(ex,
                "AI service error for Project {ProjectId}, Section {SectionId}",
                request.ProjectId, request.SectionId);

            return Result.Failure<GeneratedTextResponse>(
                Error.Failure("AiService",
                    "Error al comunicarse con el servicio de IA. Inténtelo de nuevo más tarde."));
        }
    }

    /// <summary>Maps InterventionType to the n8n webhook projectType field.</summary>
    private static string FormatProjectType(InterventionType type) => type switch
    {
        InterventionType.NewConstruction => "NewConstruction",
        InterventionType.Reform => "Reform",
        InterventionType.Extension => "Extension",
        _ => type.ToString()
    };

    /// <summary>Maps InterventionType to a human-readable Spanish label for the technical context.</summary>
    private static string FormatInterventionType(InterventionType type) => type switch
    {
        InterventionType.NewConstruction => "Obra Nueva",
        InterventionType.Reform => "Reforma",
        InterventionType.Extension => "Ampliación",
        _ => type.ToString()
    };
}
