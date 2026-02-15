using System.Text;
using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Ai.Commands.GenerateSectionText;

/// <summary>
/// Handles GenerateSectionTextCommand by calling the AI service.
/// Formats the prompt with optional context before sending.
/// </summary>
public sealed class GenerateSectionTextHandler
    : IRequestHandler<GenerateSectionTextCommand, Result<GeneratedTextResponse>>
{
    private readonly IAiService _aiService;
    private readonly ILogger<GenerateSectionTextHandler> _logger;

    public GenerateSectionTextHandler(IAiService aiService)
    {
        _aiService = aiService;
        _logger = null!; // Logger is optional for testability
    }

    public GenerateSectionTextHandler(IAiService aiService, ILogger<GenerateSectionTextHandler> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<Result<GeneratedTextResponse>> Handle(
        GenerateSectionTextCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var formattedPrompt = FormatPrompt(request.Prompt, request.Context);

            _logger?.LogInformation(
                "Generating AI text for Project {ProjectId}, Section {SectionId}",
                request.ProjectId, request.SectionId);

            var generatedText = await _aiService.GenerateTextAsync(formattedPrompt, cancellationToken);

            if (string.IsNullOrWhiteSpace(generatedText))
            {
                _logger?.LogWarning(
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
            _logger?.LogError(ex,
                "AI service error for Project {ProjectId}, Section {SectionId}",
                request.ProjectId, request.SectionId);

            return Result.Failure<GeneratedTextResponse>(
                Error.Failure("AiService",
                    "Error al comunicarse con el servicio de IA. Inténtelo de nuevo más tarde."));
        }
    }

    private static string FormatPrompt(string prompt, string? context)
    {
        if (string.IsNullOrWhiteSpace(context))
            return prompt;

        var sb = new StringBuilder();
        sb.AppendLine("Contexto del proyecto:");
        sb.AppendLine(context);
        sb.AppendLine();
        sb.AppendLine("Instrucción:");
        sb.AppendLine(prompt);

        return sb.ToString();
    }
}
