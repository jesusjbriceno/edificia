namespace Edificia.Infrastructure.TemplateStorage;

public sealed class TemplateStorageSettings
{
    public const string SectionName = "TemplateStorage";

    public string Provider { get; set; } = "local";

    public string BasePath { get; set; } = "./local_data/templates";

    /// <summary>Webhook URL for UPLOAD_TEMPLATE and DELETE_TEMPLATE operations.</summary>
    public string N8nStoreWebhookUrl { get; set; } = string.Empty;

    /// <summary>Webhook URL for GET_TEMPLATE operations.</summary>
    public string N8nRetrieveWebhookUrl { get; set; } = string.Empty;

    public string N8nApiSecret { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 60;
}
