namespace Edificia.Application.Ai.Commands.GenerateSectionText;

/// <summary>
/// Request DTO for generating AI text for a section.
/// </summary>
public sealed record GenerateTextRequest(
    string SectionId,
    string Prompt,
    string? Context);
