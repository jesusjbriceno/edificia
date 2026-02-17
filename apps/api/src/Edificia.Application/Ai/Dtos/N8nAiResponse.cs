using System.Text.Json.Serialization;

namespace Edificia.Application.Ai.Dtos;

/// <summary>
/// Response DTO received from the n8n webhook.
/// Only <see cref="GeneratedText"/> is required; <see cref="Usage"/> is optional metadata.
/// </summary>
public sealed record N8nAiResponse(
    [property: JsonPropertyName("generatedText")]
    string? GeneratedText,

    [property: JsonPropertyName("usage")]
    AiUsageMetadata? Usage);

/// <summary>
/// Optional metadata about the AI generation (model used, tokens consumed).
/// </summary>
public sealed record AiUsageMetadata(
    [property: JsonPropertyName("model")]
    string? Model,

    [property: JsonPropertyName("tokens")]
    int? Tokens);
