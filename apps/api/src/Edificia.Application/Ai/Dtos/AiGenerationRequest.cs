using System.Text.Json.Serialization;
using Edificia.Application.Ai.Commands.GenerateSectionText;
using Edificia.Domain.Entities;
using Edificia.Domain.Enums;

namespace Edificia.Application.Ai.Dtos;

/// <summary>
/// Request DTO sent to the n8n webhook for AI text generation.
/// Follows the contract defined in ESPECIFICACION_FLUJOS_N8N.md.
/// </summary>
public sealed record AiGenerationRequest(
    [property: JsonPropertyName("sectionCode")]
    string SectionCode,

    [property: JsonPropertyName("projectType")]
    string ProjectType,

    [property: JsonPropertyName("technicalContext")]
    TechnicalContext? TechnicalContext,

    [property: JsonPropertyName("userInstructions")]
    string? UserInstructions)
{
    /// <summary>
    /// Creates an AiGenerationRequest from a Project entity and a GenerateSectionTextCommand.
    /// </summary>
    public static AiGenerationRequest FromProjectAndCommand(
        Project project, GenerateSectionTextCommand command) => new(
            SectionCode: command.SectionId,
            ProjectType: FormatProjectType(project.InterventionType),
            TechnicalContext: new TechnicalContext(
                ProjectTitle: project.Title,
                InterventionType: FormatInterventionType(project.InterventionType),
                IsLoeRequired: project.IsLoeRequired,
                Address: project.Address,
                LocalRegulations: project.LocalRegulations,
                ExistingContent: command.Context),
            UserInstructions: command.Prompt);

    /// <summary>Maps InterventionType to the n8n webhook projectType field.</summary>
    internal static string FormatProjectType(InterventionType type) => type switch
    {
        InterventionType.NewConstruction => "NewConstruction",
        InterventionType.Reform => "Reform",
        InterventionType.Extension => "Extension",
        _ => type.ToString()
    };

    /// <summary>Maps InterventionType to a human-readable Spanish label for the technical context.</summary>
    internal static string FormatInterventionType(InterventionType type) => type switch
    {
        InterventionType.NewConstruction => "Obra Nueva",
        InterventionType.Reform => "Reforma",
        InterventionType.Extension => "Ampliación",
        _ => type.ToString()
    };
}

/// <summary>
/// Typed technical context sent to n8n with project metadata.
/// n8n uses this to build the AI prompt with full project awareness.
/// </summary>
public sealed record TechnicalContext(
    [property: JsonPropertyName("projectTitle")]
    string? ProjectTitle,

    [property: JsonPropertyName("interventionType")]
    string? InterventionType,

    [property: JsonPropertyName("isLoeRequired")]
    bool? IsLoeRequired,

    [property: JsonPropertyName("address")]
    string? Address,

    [property: JsonPropertyName("localRegulations")]
    string? LocalRegulations,

    [property: JsonPropertyName("existingContent")]
    string? ExistingContent);
