namespace Edificia.Application.Ai.Commands.GenerateSectionText;

/// <summary>
/// Response DTO containing generated AI text for a section.
/// </summary>
public sealed record GeneratedTextResponse(
    Guid ProjectId,
    string SectionId,
    string GeneratedText)
{
    /// <summary>
    /// Creates a GeneratedTextResponse from the command context and the AI-generated text.
    /// </summary>
    public static GeneratedTextResponse Create(
        Guid projectId, string sectionId, string generatedText) => new(
            projectId, sectionId, generatedText);
}
