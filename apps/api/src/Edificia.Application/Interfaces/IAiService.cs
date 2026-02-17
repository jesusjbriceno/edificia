using Edificia.Application.Ai.Dtos;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Service interface for AI text generation.
/// Defined in Application layer; implemented in Infrastructure (N8nAiService).
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Generates text by sending a structured request to the AI provider.
    /// </summary>
    /// <param name="request">The generation request with section code, project type, and technical context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated text response, or null if the service fails.</returns>
    Task<string?> GenerateTextAsync(AiGenerationRequest request, CancellationToken cancellationToken = default);
}
