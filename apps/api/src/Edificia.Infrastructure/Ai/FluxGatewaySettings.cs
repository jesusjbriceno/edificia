namespace Edificia.Infrastructure.Ai;

/// <summary>
/// Strongly-typed configuration for the Flux AI Gateway.
/// Bound from appsettings.json section "FluxGateway".
/// </summary>
public sealed class FluxGatewaySettings
{
    public const string SectionName = "FluxGateway";

    /// <summary>URL for OAuth2 token authentication.</summary>
    public string AuthUrl { get; set; } = string.Empty;

    /// <summary>URL for chat completions API.</summary>
    public string ChatUrl { get; set; } = string.Empty;

    /// <summary>OAuth2 client identifier for Edificia.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>OAuth2 client secret for Edificia.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Model identifier to use in chat completions (optional override).</summary>
    public string? Model { get; set; }
}
