using System.Text.Json.Serialization;

namespace Edificia.Infrastructure.Ai;

// ── Auth DTOs ──

/// <summary>Request body for Flux Gateway login.</summary>
internal sealed record FluxAuthRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password);

/// <summary>Response from Flux Gateway login.</summary>
internal sealed record FluxAuthResponse(
    [property: JsonPropertyName("token")] string Token);

// ── Chat DTOs ──

/// <summary>Request body for Flux Gateway chat completions.</summary>
internal sealed record FluxChatRequest(
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("messages")] FluxChatMessage[] Messages);

/// <summary>A single message in the chat completion request.</summary>
internal sealed record FluxChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

/// <summary>Response from Flux Gateway chat completions.</summary>
internal sealed record FluxChatResponse(
    [property: JsonPropertyName("choices")] FluxChatChoice[]? Choices);

/// <summary>A single choice in the chat completion response.</summary>
internal sealed record FluxChatChoice(
    [property: JsonPropertyName("message")] FluxChatChoiceMessage? Message);

/// <summary>The message content within a chat choice.</summary>
internal sealed record FluxChatChoiceMessage(
    [property: JsonPropertyName("content")] string? Content);
