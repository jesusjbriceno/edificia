namespace Edificia.Infrastructure.Ai;

/// <summary>
/// Configuration settings for the n8n AI webhook integration.
/// Bound from appsettings "AI" section via IOptions.
/// </summary>
public sealed class N8nAiSettings
{
    public const string SectionName = "AI";

    /// <summary>Informational label for the AI provider (default: "n8n").</summary>
    public string Provider { get; set; } = "n8n";

    /// <summary>Full URL of the n8n webhook endpoint for text generation.</summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>Shared secret sent via X-Edificia-Auth header. Must match the n8n webhook validation.</summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>HTTP client timeout in seconds (default: 120).</summary>
    public int TimeoutSeconds { get; set; } = 120;
}
