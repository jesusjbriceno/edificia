using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Edificia.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Edificia.Infrastructure.Export;

/// <summary>
/// Exports project content tree to a .docx file using the OpenXML SDK.
/// Transforms the JSON tree (chapters → sections with HTML content) into a structured Word document.
/// </summary>
public sealed partial class DocxExportService : IDocumentExportService
{
    private readonly ILogger<DocxExportService>? _logger;

    public DocxExportService() { }

    public DocxExportService(ILogger<DocxExportService> logger) => _logger = logger;

    public Task<byte[]> ExportToDocxAsync(ExportDocumentData data, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();

        using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // ── Document styles ──
            AddStyleDefinitions(mainPart);

            // ── Title page ──
            AddTitlePage(body, data);

            // ── Page break after title ──
            body.AppendChild(new Paragraph(
                new Run(new Break { Type = BreakValues.Page })));

            // ── Content tree ──
            ProcessContentTree(body, data.ContentTreeJson);

            mainPart.Document.Save();
        }

        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> ExportToDocxWithTemplateAsync(
        ExportDocumentData data,
        byte[] templateContent,
        CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(templateContent.ToArray());

        using (var wordDoc = WordprocessingDocument.Open(stream, true))
        {
            wordDoc.ChangeDocumentType(WordprocessingDocumentType.Document);

            var mainPart = wordDoc.MainDocumentPart ?? wordDoc.AddMainDocumentPart();

            if (mainPart.Document is null)
            {
                mainPart.Document = new Document(new Body());
            }

            var body = mainPart.Document.Body ?? mainPart.Document.AppendChild(new Body());

            body.AppendChild(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
            AddTitlePage(body, data);
            ProcessContentTree(body, data.ContentTreeJson);
            ForceUpdateFieldsOnOpen(mainPart);

            mainPart.Document.Save();
        }

        return Task.FromResult(stream.ToArray());
    }

    private static void AddStyleDefinitions(MainDocumentPart mainPart)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        var styles = new Styles();

        // Heading 1 style
        styles.AppendChild(CreateHeadingStyle("Heading1", "Título 1", "28", "1F3864"));
        // Heading 2 style
        styles.AppendChild(CreateHeadingStyle("Heading2", "Título 2", "24", "2E75B6"));
        // Heading 3 style
        styles.AppendChild(CreateHeadingStyle("Heading3", "Título 3", "22", "404040"));

        stylesPart.Styles = styles;
    }

    private static Style CreateHeadingStyle(string styleId, string styleName, string fontSize, string color)
    {
        return new Style(
            new StyleName { Val = styleName },
            new BasedOn { Val = "Normal" },
            new NextParagraphStyle { Val = "Normal" },
            new StyleRunProperties(
                new Bold(),
                new Color { Val = color },
                new FontSize { Val = fontSize },
                new FontSizeComplexScript { Val = fontSize }))
        {
            Type = StyleValues.Paragraph,
            StyleId = styleId,
            CustomStyle = true
        };
    }

    private static void AddTitlePage(Body body, ExportDocumentData data)
    {
        // Main title
        var titleParagraph = new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { After = "400" }),
            new Run(
                new RunProperties(
                    new Bold(),
                    new FontSize { Val = "48" },
                    new FontSizeComplexScript { Val = "48" },
                    new Color { Val = "1F3864" }),
                new Text("MEMORIA DE PROYECTO")));
        body.AppendChild(titleParagraph);

        // Project title
        var projectTitle = new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { After = "600" }),
            new Run(
                new RunProperties(
                    new Bold(),
                    new FontSize { Val = "36" },
                    new FontSizeComplexScript { Val = "36" }),
                new Text(data.Title)));
        body.AppendChild(projectTitle);

        // Separator
        body.AppendChild(new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Center }),
            new Run(new Text("────────────────────────────"))));

        // Metadata
        AddMetadataLine(body, "Tipo de intervención", data.InterventionType);
        AddMetadataLine(body, "Sujeto a LOE", data.IsLoeRequired ? "Sí" : "No");

        if (!string.IsNullOrWhiteSpace(data.Address))
            AddMetadataLine(body, "Dirección", data.Address);

        // Generation date
        AddMetadataLine(body, "Fecha de exportación", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm") + " UTC");
    }

    private static void AddMetadataLine(Body body, string label, string value)
    {
        body.AppendChild(new Paragraph(
            new ParagraphProperties(
                new SpacingBetweenLines { After = "100" }),
            new Run(
                new RunProperties(new Bold()),
                new Text(label + ": ") { Space = SpaceProcessingModeValues.Preserve }),
            new Run(
                new Text(value))));
    }

    private void ProcessContentTree(Body body, string contentTreeJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(contentTreeJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("chapters", out var chapters) &&
                chapters.ValueKind == JsonValueKind.Array)
            {
                foreach (var chapter in chapters.EnumerateArray())
                {
                    ProcessNode(body, chapter, level: 1);
                }
            }
            else
            {
                _logger?.LogWarning("Content tree has no 'chapters' array property");
            }
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "Failed to parse content tree JSON");
            body.AppendChild(new Paragraph(
                new Run(new Text("[Error al procesar el contenido del documento]"))));
        }
    }

    private void ProcessNode(Body body, JsonElement node, int level)
    {
        // Extract title
        var title = node.TryGetProperty("title", out var titleElement)
            ? titleElement.GetString() ?? string.Empty
            : string.Empty;

        // Add heading
        if (!string.IsNullOrWhiteSpace(title))
        {
            var headingStyle = level switch
            {
                1 => "Heading1",
                2 => "Heading2",
                _ => "Heading3"
            };

            body.AppendChild(new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId { Val = headingStyle }),
                new Run(new Text(title))));
        }

        // Extract and render content (HTML → Word paragraphs)
        if (node.TryGetProperty("content", out var contentElement))
        {
            var content = contentElement.GetString();
            if (!string.IsNullOrWhiteSpace(content))
            {
                RenderHtmlContent(body, content);
            }
        }

        // Process child sections recursively
        if (node.TryGetProperty("sections", out var sections) &&
            sections.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in sections.EnumerateArray())
            {
                ProcessNode(body, child, level + 1);
            }
        }
    }

    /// <summary>
    /// Converts simple HTML content (from TipTap editor) into Word paragraphs.
    /// Handles: paragraphs, bold, italic, underline, headings, lists.
    /// </summary>
    private static void RenderHtmlContent(Body body, string html)
    {
        // Split by block-level tags
        var blocks = BlockSplitRegex().Split(html);

        foreach (var block in blocks)
        {
            if (string.IsNullOrWhiteSpace(block)) continue;

            var trimmed = block.Trim();

            // Handle list items
            if (trimmed.StartsWith("<li", StringComparison.OrdinalIgnoreCase))
            {
                var listParagraph = new Paragraph(
                    new ParagraphProperties(
                        new NumberingProperties(
                            new NumberingLevelReference { Val = 0 },
                            new NumberingId { Val = 1 }),
                        new SpacingBetweenLines { After = "60" }));

                foreach (var run in CreateFormattedRuns(trimmed))
                    listParagraph.AppendChild(run);

                body.AppendChild(listParagraph);
                continue;
            }

            // Handle headings within content
            var headingMatch = ContentHeadingRegex().Match(trimmed);
            if (headingMatch.Success)
            {
                var headingLevel = headingMatch.Groups[1].Value;
                var headingText = StripHtmlTags(headingMatch.Groups[2].Value);
                var styleId = headingLevel switch
                {
                    "1" => "Heading1",
                    "2" => "Heading2",
                    _ => "Heading3"
                };

                body.AppendChild(new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId { Val = styleId }),
                    new Run(new Text(headingText))));
                continue;
            }

            // Default: paragraph with inline formatting
            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines { After = "120" }));

            foreach (var run in CreateFormattedRuns(trimmed))
            {
                paragraph.AppendChild(run);
            }

            body.AppendChild(paragraph);
        }
    }

    /// <summary>
    /// Creates Run elements with formatting from inline HTML tags (bold, italic, underline).
    /// </summary>
    private static IEnumerable<Run> CreateFormattedRuns(string html)
    {
        var plainText = StripHtmlTags(html);
        if (string.IsNullOrWhiteSpace(plainText))
            yield break;

        // Detect inline formatting from the HTML
        var isBold = html.Contains("<strong", StringComparison.OrdinalIgnoreCase) ||
                     html.Contains("<b>", StringComparison.OrdinalIgnoreCase);
        var isItalic = html.Contains("<em", StringComparison.OrdinalIgnoreCase) ||
                       html.Contains("<i>", StringComparison.OrdinalIgnoreCase);
        var isUnderline = html.Contains("<u>", StringComparison.OrdinalIgnoreCase) ||
                          html.Contains("<u ", StringComparison.OrdinalIgnoreCase);

        var runProperties = new RunProperties();

        if (isBold) runProperties.AppendChild(new Bold());
        if (isItalic) runProperties.AppendChild(new Italic());
        if (isUnderline) runProperties.AppendChild(new Underline { Val = UnderlineValues.Single });

        yield return new Run(runProperties, new Text(plainText) { Space = SpaceProcessingModeValues.Preserve });
    }

    private static string StripHtmlTags(string html)
    {
        var text = HtmlTagRegex().Replace(html, string.Empty);
        text = System.Net.WebUtility.HtmlDecode(text);
        return text.Trim();
    }

    [GeneratedRegex(@"</?(?:p|div|ul|ol|br|hr)[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockSplitRegex();

    [GeneratedRegex(@"<h([1-6])[^>]*>(.*?)</h\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ContentHeadingRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    private static void ForceUpdateFieldsOnOpen(MainDocumentPart mainPart)
    {
        var settingsPart = mainPart.DocumentSettingsPart ?? mainPart.AddNewPart<DocumentSettingsPart>();

        if (settingsPart.Settings is null)
        {
            settingsPart.Settings = new Settings();
        }

        var hasUpdateFields = settingsPart.Settings.Elements<UpdateFieldsOnOpen>().Any();
        if (!hasUpdateFields)
        {
            settingsPart.Settings.Append(new UpdateFieldsOnOpen { Val = true });
            settingsPart.Settings.Save();
        }
    }
}
