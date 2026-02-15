namespace Edificia.Application.Ai.Services;

/// <summary>
/// Service for building contextualized AI prompts from project data.
/// Defined and implemented in Application layer (pure string formatting, no external deps).
/// </summary>
public interface IPromptTemplateService
{
    /// <summary>
    /// Builds a fully contextualized prompt for section text generation.
    /// Injects project metadata (intervention type, LOE status, regulations) into the prompt.
    /// </summary>
    /// <param name="context">The section prompt context with project data.</param>
    /// <returns>A formatted prompt string ready to send to the AI service.</returns>
    string BuildSectionPrompt(SectionPromptContext context);
}
