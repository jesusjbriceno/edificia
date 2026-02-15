using Edificia.Domain.Enums;

namespace Edificia.Application.Ai.Services;

/// <summary>
/// Encapsulates all project data needed to build a contextualized AI prompt.
/// Used by <see cref="IPromptTemplateService"/> to inject project-specific context.
/// </summary>
public sealed record SectionPromptContext(
    string SectionId,
    string UserPrompt,
    string? ExistingContent,
    string ProjectTitle,
    InterventionType InterventionType,
    bool IsLoeRequired,
    string? Address,
    string? LocalRegulations);
