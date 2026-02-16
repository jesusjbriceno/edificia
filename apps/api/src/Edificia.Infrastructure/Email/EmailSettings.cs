namespace Edificia.Infrastructure.Email;

/// <summary>
/// Email provider configuration.
/// Supports SMTP and Brevo (external API) providers.
/// Mapped from "Email" configuration section.
/// </summary>
public sealed class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>
    /// Email provider to use: "Smtp" or "Brevo".
    /// Default: "Smtp".
    /// </summary>
    public string Provider { get; set; } = "Smtp";

    /// <summary>Sender email address (From).</summary>
    public string FromAddress { get; set; } = "noreply@edificia.dev";

    /// <summary>Sender display name.</summary>
    public string FromName { get; set; } = "EDIFICIA";

    // ── SMTP Settings ──

    /// <summary>SMTP server host.</summary>
    public string SmtpHost { get; set; } = "localhost";

    /// <summary>SMTP server port.</summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>SMTP username.</summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>SMTP password.</summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>Use SSL/TLS for SMTP connection.</summary>
    public bool SmtpUseSsl { get; set; } = true;

    // ── Brevo Settings ──

    /// <summary>Brevo API key.</summary>
    public string BrevoApiKey { get; set; } = string.Empty;

    /// <summary>Brevo API base URL.</summary>
    public string BrevoApiUrl { get; set; } = "https://api.brevo.com/v3";
}
