namespace Edificia.Application.Interfaces;

/// <summary>
/// Service interface for AI text generation.
/// Defined in Application layer; implemented in Infrastructure (FluxAiService).
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Generates text based on the provided prompt.
    /// </summary>
    /// <param name="prompt">The formatted prompt to send to the AI service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated text response, or null if the service fails.</returns>
    Task<string?> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);
}
