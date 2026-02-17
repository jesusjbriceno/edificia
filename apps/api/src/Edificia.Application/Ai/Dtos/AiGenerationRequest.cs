using System.Text.Json.Serialization;

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
    string? UserInstructions);

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
