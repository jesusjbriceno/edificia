using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;
using DocMath = DocumentFormat.OpenXml.Math;
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
            EnsureNumberingDefinitions(mainPart);

            // ── Title page ──
            AddTitlePage(body, data);

            // ── Page break after title ──
            body.AppendChild(new Paragraph(
                new Run(new Break { Type = BreakValues.Page })));

            // ── Content tree ──
            ProcessContentTree(body, mainPart, data.ContentTreeJson);

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
            EnsureNumberingDefinitions(mainPart);

            body.AppendChild(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
            AddTitlePage(body, data);
            ProcessContentTree(body, mainPart, data.ContentTreeJson);
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

    private void ProcessContentTree(Body body, MainDocumentPart mainPart, string contentTreeJson)
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
                    ProcessNode(body, mainPart, chapter, level: 1);
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

    private void ProcessNode(Body body, MainDocumentPart mainPart, JsonElement node, int level)
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
                RenderHtmlContent(body, mainPart, content);
            }
        }

        // Process child sections recursively
        if (node.TryGetProperty("sections", out var sections) &&
            sections.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in sections.EnumerateArray())
            {
                ProcessNode(body, mainPart, child, level + 1);
            }
        }
    }

    /// <summary>
    /// Converts simple HTML content (from TipTap editor) into Word paragraphs.
    /// Handles: paragraphs, bold, italic, underline, headings, lists.
    /// </summary>
    private static void RenderHtmlContent(Body body, MainDocumentPart mainPart, string html)
    {
        var blocks = SplitHtmlPreservingTables(html);

        foreach (var block in blocks)
        {
            if (string.IsNullOrWhiteSpace(block)) continue;

            var trimmed = block.Trim();

            if (trimmed.StartsWith("<table", StringComparison.OrdinalIgnoreCase))
            {
                var table = CreateTableFromHtml(trimmed);
                if (table is not null)
                {
                    body.AppendChild(table);
                    body.AppendChild(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { After = "120" })));
                }

                continue;
            }

            // Handle list items
            if (trimmed.StartsWith("<li", StringComparison.OrdinalIgnoreCase))
            {
                var listParagraph = new Paragraph(
                    new ParagraphProperties(
                        new NumberingProperties(
                            new NumberingLevelReference { Val = 0 },
                            new NumberingId { Val = 1 }),
                        new SpacingBetweenLines { After = "60" }));

                foreach (var element in CreateFormattedElements(mainPart, trimmed))
                    listParagraph.AppendChild(element);

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

            var plainText = StripHtmlTags(trimmed);
            var blockMathMatch = BlockMathRegex().Match(plainText);
            if (blockMathMatch.Success)
            {
                var expression = NormalizeMathExpression(blockMathMatch.Groups["expr"].Value);
                if (!string.IsNullOrWhiteSpace(expression))
                {
                    body.AppendChild(CreateBlockMathParagraph(expression));
                }

                continue;
            }

            // Default: paragraph with inline formatting
            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines { After = "120" }));

            foreach (var element in CreateFormattedElements(mainPart, trimmed))
            {
                paragraph.AppendChild(element);
            }

            body.AppendChild(paragraph);
        }
    }

    /// <summary>
    /// Creates Run elements with formatting from inline HTML tags (bold, italic, underline).
    /// </summary>
    private static IEnumerable<OpenXmlElement> CreateFormattedElements(MainDocumentPart mainPart, string html)
    {
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

        var anchorMatches = AnchorRegex().Matches(html);
        if (anchorMatches.Count == 0)
        {
            var plainText = StripHtmlTags(html);
            foreach (var run in CreateRunsWithMath(plainText, runProperties))
            {
                yield return run;
            }

            yield break;
        }

        var currentIndex = 0;
        foreach (Match match in anchorMatches)
        {
            if (match.Index > currentIndex)
            {
                var beforeText = StripHtmlTags(html[currentIndex..match.Index]);
                foreach (var run in CreateRunsWithMath(beforeText, runProperties))
                {
                    yield return run;
                }
            }

            var href = match.Groups["href"].Value;
            var anchorText = StripHtmlTags(match.Groups["text"].Value);
            if (!string.IsNullOrWhiteSpace(anchorText))
            {
                var hyperlink = CreateHyperlink(mainPart, href, anchorText, runProperties);
                if (hyperlink is not null)
                {
                    yield return hyperlink;
                }
                else
                {
                    foreach (var run in CreateRunsWithMath(anchorText, runProperties))
                    {
                        yield return run;
                    }
                }
            }

            currentIndex = match.Index + match.Length;
        }

        if (currentIndex < html.Length)
        {
            var afterText = StripHtmlTags(html[currentIndex..]);
            foreach (var run in CreateRunsWithMath(afterText, runProperties))
            {
                yield return run;
            }
        }
    }

    private static IEnumerable<OpenXmlElement> CreateRunsWithMath(string text, RunProperties baseRunProperties)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        var matches = InlineMathRegex().Matches(text);
        if (matches.Count == 0)
        {
            yield return CreateTextRun(text, baseRunProperties);
            yield break;
        }

        var currentIndex = 0;
        foreach (Match match in matches)
        {
            if (match.Index > currentIndex)
            {
                var beforeText = text[currentIndex..match.Index];
                if (!string.IsNullOrWhiteSpace(beforeText))
                {
                    yield return CreateTextRun(beforeText, baseRunProperties);
                }
            }

            var expression = NormalizeMathExpression(match.Groups["expr"].Value);
            if (!string.IsNullOrWhiteSpace(expression))
            {
                yield return CreateOfficeMathExpression(expression);
            }

            currentIndex = match.Index + match.Length;
        }

        if (currentIndex < text.Length)
        {
            var afterText = text[currentIndex..];
            if (!string.IsNullOrWhiteSpace(afterText))
            {
                yield return CreateTextRun(afterText, baseRunProperties);
            }
        }
    }

    private static Run CreateTextRun(string text, RunProperties baseRunProperties)
        => new(
            (RunProperties)baseRunProperties.CloneNode(true),
            new Text(text) { Space = SpaceProcessingModeValues.Preserve });

    private static Run CreateMathRun(string expression, RunProperties baseRunProperties)
    {
        var mathRunProperties = (RunProperties)baseRunProperties.CloneNode(true);
        mathRunProperties.AppendChild(new RunFonts { Ascii = "Cambria Math", HighAnsi = "Cambria Math" });
        mathRunProperties.AppendChild(new Italic());

        return new Run(
            mathRunProperties,
            new Text(expression) { Space = SpaceProcessingModeValues.Preserve });
    }

    private static Paragraph CreateBlockMathParagraph(string expression)
    {
        var paragraph = new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { Before = "80", After = "120" }));

        paragraph.AppendChild(CreateOfficeMathExpression(expression));
        return paragraph;
    }

    private static DocMath.OfficeMath CreateOfficeMathExpression(string expression)
    {
        var officeMath = new DocMath.OfficeMath();

        if (TryParseSummation(expression, out var sumSub, out var sumSup, out var sumBody))
        {
            officeMath.AppendChild(CreateSummationNode(sumSub, sumSup));

            if (!string.IsNullOrWhiteSpace(sumBody))
            {
                officeMath.AppendChild(CreateMathNode(sumBody));
            }

            return officeMath;
        }

        officeMath.AppendChild(CreateMathNode(expression));
        return officeMath;
    }

    private static OpenXmlElement CreateMathNode(string expression)
    {
        var normalized = NormalizeMathExpression(expression);

        if (TryParseFraction(normalized, out var numerator, out var denominator))
        {
            return CreateFractionNode(numerator, denominator);
        }

        if (TryParseSubSup(normalized, out var baseExprSubSup, out var subExpr, out var supExpr))
        {
            return CreateSubSuperscriptNode(baseExprSubSup, subExpr, supExpr);
        }

        if (TryParseSuperscript(normalized, out var baseExprSup, out var superExpr))
        {
            return CreateSuperscriptNode(baseExprSup, superExpr);
        }

        if (TryParseSubscript(normalized, out var baseExprSub, out var subOnlyExpr))
        {
            return CreateSubscriptNode(baseExprSub, subOnlyExpr);
        }

        return CreateMathTextRun(normalized);
    }

    private static DocMath.Fraction CreateFractionNode(string numerator, string denominator)
    {
        var fraction = new DocMath.Fraction();
        var num = new DocMath.Numerator();
        var den = new DocMath.Denominator();

        num.AppendChild(CreateMathBaseElement(numerator));
        den.AppendChild(CreateMathBaseElement(denominator));

        fraction.AppendChild(num);
        fraction.AppendChild(den);

        return fraction;
    }

    private static DocMath.SubSuperscript CreateSubSuperscriptNode(string baseExpr, string subExpr, string supExpr)
    {
        var node = new DocMath.SubSuperscript();

        var baseElement = new DocMath.Base();
        baseElement.AppendChild(CreateMathNode(baseExpr));

        var subElement = new DocMath.SubArgument();
        subElement.AppendChild(CreateMathBaseElement(subExpr));

        var supElement = new DocMath.SuperArgument();
        supElement.AppendChild(CreateMathBaseElement(supExpr));

        node.AppendChild(baseElement);
        node.AppendChild(subElement);
        node.AppendChild(supElement);

        return node;
    }

    private static DocMath.Superscript CreateSuperscriptNode(string baseExpr, string superExpr)
    {
        var node = new DocMath.Superscript();

        var baseElement = new DocMath.Base();
        baseElement.AppendChild(CreateMathNode(baseExpr));

        var superElement = new DocMath.SuperArgument();
        superElement.AppendChild(CreateMathBaseElement(superExpr));

        node.AppendChild(baseElement);
        node.AppendChild(superElement);

        return node;
    }

    private static DocMath.Subscript CreateSubscriptNode(string baseExpr, string subExpr)
    {
        var node = new DocMath.Subscript();

        var baseElement = new DocMath.Base();
        baseElement.AppendChild(CreateMathNode(baseExpr));

        var subElement = new DocMath.SubArgument();
        subElement.AppendChild(CreateMathBaseElement(subExpr));

        node.AppendChild(baseElement);
        node.AppendChild(subElement);

        return node;
    }

    private static DocMath.SubSuperscript CreateSummationNode(string subExpr, string supExpr)
        => CreateSubSuperscriptNode("∑", subExpr, supExpr);

    private static DocMath.Base CreateMathBaseElement(string expression)
    {
        var baseElement = new DocMath.Base();
        baseElement.AppendChild(CreateMathNode(UnwrapBraces(expression)));
        return baseElement;
    }

    private static DocMath.Run CreateMathTextRun(string text)
    {
        var mathRun = new DocMath.Run();
        mathRun.AppendChild(new DocMath.Text(text));
        return mathRun;
    }

    private static string UnwrapBraces(string expression)
    {
        var trimmed = NormalizeMathExpression(expression);
        if (trimmed.Length >= 2 && trimmed.StartsWith('{') && trimmed.EndsWith('}'))
        {
            return trimmed[1..^1];
        }

        return trimmed;
    }

    private static bool TryParseFraction(string expression, out string numerator, out string denominator)
    {
        var match = FractionMathRegex().Match(expression);
        if (match.Success)
        {
            numerator = match.Groups["num"].Value;
            denominator = match.Groups["den"].Value;
            return true;
        }

        numerator = string.Empty;
        denominator = string.Empty;
        return false;
    }

    private static bool TryParseSummation(string expression, out string subExpr, out string supExpr, out string bodyExpr)
    {
        var match = SummationMathRegex().Match(expression);
        if (match.Success)
        {
            subExpr = match.Groups["sub"].Value;
            supExpr = match.Groups["sup"].Value;
            bodyExpr = match.Groups["body"].Value;
            return true;
        }

        subExpr = string.Empty;
        supExpr = string.Empty;
        bodyExpr = string.Empty;
        return false;
    }

    private static bool TryParseSubSup(string expression, out string baseExpr, out string subExpr, out string supExpr)
    {
        var match = SubSupMathRegex().Match(expression);
        if (match.Success)
        {
            baseExpr = match.Groups["base"].Value;
            subExpr = match.Groups["sub"].Value;
            supExpr = match.Groups["sup"].Value;
            return true;
        }

        baseExpr = string.Empty;
        subExpr = string.Empty;
        supExpr = string.Empty;
        return false;
    }

    private static bool TryParseSuperscript(string expression, out string baseExpr, out string supExpr)
    {
        var match = SupMathRegex().Match(expression);
        if (match.Success)
        {
            baseExpr = match.Groups["base"].Value;
            supExpr = match.Groups["sup"].Value;
            return true;
        }

        baseExpr = string.Empty;
        supExpr = string.Empty;
        return false;
    }

    private static bool TryParseSubscript(string expression, out string baseExpr, out string subExpr)
    {
        var match = SubMathRegex().Match(expression);
        if (match.Success)
        {
            baseExpr = match.Groups["base"].Value;
            subExpr = match.Groups["sub"].Value;
            return true;
        }

        baseExpr = string.Empty;
        subExpr = string.Empty;
        return false;
    }

    private static string NormalizeMathExpression(string expression)
        => System.Net.WebUtility.HtmlDecode(expression).Trim();

    private static void EnsureNumberingDefinitions(MainDocumentPart mainPart)
    {
        var numberingPart = mainPart.NumberingDefinitionsPart ?? mainPart.AddNewPart<NumberingDefinitionsPart>();
        var numbering = numberingPart.Numbering ?? new Numbering();

        if (!numbering.Elements<AbstractNum>().Any(n => n.AbstractNumberId?.Value == 1))
        {
            var bulletAbstract = new AbstractNum(
                new Level(
                    new NumberingFormat { Val = NumberFormatValues.Bullet },
                    new LevelText { Val = "•" },
                    new LevelJustification { Val = LevelJustificationValues.Left },
                    new PreviousParagraphProperties(new Indentation { Left = "720", Hanging = "360" }))
                { LevelIndex = 0 })
            { AbstractNumberId = 1 };

            numbering.AppendChild(bulletAbstract);
        }

        if (!numbering.Elements<NumberingInstance>().Any(n => n.NumberID?.Value == 1))
        {
            var bulletInstance = new NumberingInstance(new AbstractNumId { Val = 1 }) { NumberID = 1 };
            numbering.AppendChild(bulletInstance);
        }

        numberingPart.Numbering = numbering;
        numberingPart.Numbering.Save();
    }

    private static Hyperlink? CreateHyperlink(MainDocumentPart mainPart, string href, string text, RunProperties baseRunProperties)
    {
        if (!TryCreateAllowedUri(href, out var uri))
        {
            return null;
        }

        var relationship = mainPart.AddHyperlinkRelationship(uri, true);

        var linkRunProperties = (RunProperties)baseRunProperties.CloneNode(true);
        linkRunProperties.AppendChild(new Color { Val = "0563C1" });
        linkRunProperties.AppendChild(new Underline { Val = UnderlineValues.Single });

        var linkRun = new Run(
            linkRunProperties,
            new Text(text) { Space = SpaceProcessingModeValues.Preserve });

        return new Hyperlink(linkRun)
        {
            Id = relationship.Id,
            History = OnOffValue.FromBoolean(true)
        };
    }

    private static bool TryCreateAllowedUri(string href, out Uri uri)
    {
        uri = null!;

        if (string.IsNullOrWhiteSpace(href) ||
            !Uri.TryCreate(href.Trim(), UriKind.Absolute, out var parsed))
        {
            return false;
        }

        if (parsed.Scheme is not ("http" or "https" or "mailto"))
        {
            return false;
        }

        uri = parsed;
        return true;
    }

    private static IEnumerable<string> SplitHtmlPreservingTables(string html)
    {
        var matches = TableRegex().Matches(html);
        if (matches.Count == 0)
        {
            return BlockSplitRegex().Split(html);
        }

        var blocks = new List<string>();
        var currentIndex = 0;

        foreach (Match match in matches)
        {
            if (match.Index > currentIndex)
            {
                blocks.AddRange(BlockSplitRegex().Split(html[currentIndex..match.Index]));
            }

            blocks.Add(match.Value);
            currentIndex = match.Index + match.Length;
        }

        if (currentIndex < html.Length)
        {
            blocks.AddRange(BlockSplitRegex().Split(html[currentIndex..]));
        }

        return blocks;
    }

    private static Table? CreateTableFromHtml(string htmlTable)
    {
        var rowMatches = TableRowRegex().Matches(htmlTable);
        if (rowMatches.Count == 0)
        {
            return null;
        }

        var table = new Table();
        table.AppendChild(new TableProperties(
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 8 },
                new BottomBorder { Val = BorderValues.Single, Size = 8 },
                new LeftBorder { Val = BorderValues.Single, Size = 8 },
                new RightBorder { Val = BorderValues.Single, Size = 8 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 6 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 6 })));

        foreach (Match rowMatch in rowMatches)
        {
            var row = new TableRow();
            var cellMatches = TableCellRegex().Matches(rowMatch.Groups["cells"].Value);
            foreach (Match cellMatch in cellMatches)
            {
                var tagName = cellMatch.Groups["tag"].Value;
                var cellText = StripHtmlTags(cellMatch.Groups["content"].Value);

                var runProperties = new RunProperties();
                if (string.Equals(tagName, "th", StringComparison.OrdinalIgnoreCase))
                {
                    runProperties.AppendChild(new Bold());
                }

                var paragraph = new Paragraph(new Run(runProperties, new Text(cellText)));
                row.AppendChild(new TableCell(
                    paragraph,
                    new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto })));
            }

            if (row.ChildElements.Count > 0)
            {
                table.AppendChild(row);
            }
        }

        return table.ChildElements.Count > 0 ? table : null;
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

    [GeneratedRegex(@"<a\b[^>]*?href\s*=\s*['""](?<href>[^'""]+)['""][^>]*>(?<text>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AnchorRegex();

    [GeneratedRegex(@"<table\b[^>]*>.*?</table>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TableRegex();

    [GeneratedRegex(@"<tr\b[^>]*>(?<cells>.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TableRowRegex();

    [GeneratedRegex(@"<(?<tag>th|td)\b[^>]*>(?<content>.*?)</\k<tag>>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TableCellRegex();

    [GeneratedRegex(@"^\s*\$\$(?<expr>[\s\S]+?)\$\$\s*$", RegexOptions.Singleline)]
    private static partial Regex BlockMathRegex();

    [GeneratedRegex(@"\$(?<expr>[^$\r\n]+)\$")]
    private static partial Regex InlineMathRegex();

    [GeneratedRegex(@"^\\frac\{(?<num>.+)\}\{(?<den>.+)\}$", RegexOptions.Singleline)]
    private static partial Regex FractionMathRegex();

    [GeneratedRegex(@"^\\sum_\{(?<sub>[^{}]+)\}\^\{(?<sup>[^{}]+)\}\s*(?<body>.*)$", RegexOptions.Singleline)]
    private static partial Regex SummationMathRegex();

    [GeneratedRegex(@"^(?<base>[A-Za-z0-9]+)_\{(?<sub>[^{}]+)\}\^\{(?<sup>[^{}]+)\}$", RegexOptions.Singleline)]
    private static partial Regex SubSupMathRegex();

    [GeneratedRegex(@"^(?<base>[A-Za-z0-9]+)\^\{?(?<sup>[A-Za-z0-9+\-*/=().]+)\}?$", RegexOptions.Singleline)]
    private static partial Regex SupMathRegex();

    [GeneratedRegex(@"^(?<base>[A-Za-z0-9]+)_\{?(?<sub>[A-Za-z0-9+\-*/=().]+)\}?$", RegexOptions.Singleline)]
    private static partial Regex SubMathRegex();

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
