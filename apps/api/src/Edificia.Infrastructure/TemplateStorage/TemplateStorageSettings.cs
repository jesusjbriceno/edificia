namespace Edificia.Infrastructure.TemplateStorage;

public sealed class TemplateStorageSettings
{
    public const string SectionName = "TemplateStorage";

    public string Provider { get; set; } = "local";

    public string BasePath { get; set; } = "./local_data/templates";

    public string N8nWebhookUrl { get; set; } = string.Empty;

    public string N8nApiSecret { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 60;
}
