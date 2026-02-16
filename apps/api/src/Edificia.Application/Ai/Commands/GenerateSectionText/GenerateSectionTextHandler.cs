using Edificia.Application.Ai.Services;
using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Ai.Commands.GenerateSectionText;

/// <summary>
/// Handles GenerateSectionTextCommand by loading the project context,
/// building a contextualized prompt via IPromptTemplateService,
/// and calling the AI service to generate section text.
/// </summary>
public sealed class GenerateSectionTextHandler
    : IRequestHandler<GenerateSectionTextCommand, Result<GeneratedTextResponse>>
{
    private readonly IAiService _aiService;
    private readonly IProjectRepository _repository;
    private readonly IPromptTemplateService _templateService;
    private readonly ILogger<GenerateSectionTextHandler> _logger;

    public GenerateSectionTextHandler(
        IAiService aiService,
        IProjectRepository repository,
        IPromptTemplateService templateService,
        ILogger<GenerateSectionTextHandler> logger)
    {
        _aiService = aiService;
        _repository = repository;
        _templateService = templateService;
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

        var promptContext = new SectionPromptContext(
            SectionId: request.SectionId,
            UserPrompt: request.Prompt,
            ExistingContent: request.Context,
            ProjectTitle: project.Title,
            InterventionType: project.InterventionType,
            IsLoeRequired: project.IsLoeRequired,
            Address: project.Address,
            LocalRegulations: project.LocalRegulations);

        var formattedPrompt = _templateService.BuildSectionPrompt(promptContext);

        try
        {
            _logger.LogInformation(
                "Generating AI text for Project {ProjectId}, Section {SectionId}",
                request.ProjectId, request.SectionId);

            var generatedText = await _aiService.GenerateTextAsync(formattedPrompt, cancellationToken);

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
}
