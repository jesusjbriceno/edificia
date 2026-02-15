namespace Edificia.Application.Ai.Commands.GenerateSectionText;

/// <summary>
/// Response DTO containing generated AI text for a section.
/// </summary>
public sealed record GeneratedTextResponse(
    Guid ProjectId,
    string SectionId,
    string GeneratedText);
