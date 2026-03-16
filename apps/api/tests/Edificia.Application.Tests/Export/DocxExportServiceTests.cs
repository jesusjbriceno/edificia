using System.IO;
using DocMath = DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Edificia.Application.Interfaces;
using Edificia.Infrastructure.Export;
using FluentAssertions;

namespace Edificia.Application.Tests.Export;

public class DocxExportServiceTests
{
  [Fact]
  public async Task ExportToDocxWithTemplateAsync_ShouldReplaceTaggedContentControls()
  {
    var service = new DocxExportService();
    var data = CreateData("<p>Contenido para MD.01</p>");
    var templateBytes = CreateDotxTemplateWithTags("ProjectTitle", "MD.01", "MC.01");

    var bytes = await service.ExportToDocxWithTemplateAsync(data, templateBytes, CancellationToken.None);

    using var stream = new MemoryStream(bytes);
    using var doc = WordprocessingDocument.Open(stream, false);

    var bodyText = doc.MainDocumentPart!.Document!.Body!.InnerText;
    bodyText.Should().Contain("Proyecto de prueba");
    bodyText.Should().Contain("Contenido para MD.01");
    bodyText.Should().NotContain("{{ProjectTitle}}");
    bodyText.Should().NotContain("{{MD.01}}");
  }

  [Fact]
  public async Task ExportToDocxWithTemplateAsync_ShouldNotAppendLegacyTitlePage()
  {
    var service = new DocxExportService();
    var data = CreateData("<p>Texto de memoria</p>");
    var templateBytes = CreateDotxTemplateWithTags("ProjectTitle", "MD.01", "MC.01");

    var bytes = await service.ExportToDocxWithTemplateAsync(data, templateBytes, CancellationToken.None);

    using var stream = new MemoryStream(bytes);
    using var doc = WordprocessingDocument.Open(stream, false);

    var bodyText = doc.MainDocumentPart!.Document!.Body!.InnerText;
    bodyText.Should().NotContain("MEMORIA DE PROYECTO");
  }

  [Fact]
  public async Task ExportToDocxWithTemplateAsync_ShouldRegenerateBody_WhenTemplateHasNoContentControls()
  {
    // Templates without SDT content controls always get their body cleared and regenerated
    // with the project title page + content tree (hybrid path). Brace placeholders in the
    // body are irrelevant — the body is replaced entirely.
    var service = new DocxExportService();
    var data = CreateData("<p>Contenido base</p>") with
    {
      PlaceholderReplacements = new Dictionary<string, string>
      {
        ["PROJECT_TITLE"] = "Proyecto Placeholder",
        ["PROJECT_ADDRESS"] = "Calle Placeholder 99"
      }
    };

    var templateBytes = CreateDotxTemplateWithPlainText(
      "Título: {{PROJECT_TITLE}}",
      "Dirección: {{PROJECT_ADDRESS}}");

    var bytes = await service.ExportToDocxWithTemplateAsync(data, templateBytes, CancellationToken.None);

    using var stream = new MemoryStream(bytes);
    using var doc = WordprocessingDocument.Open(stream, false);

    var bodyText = doc.MainDocumentPart!.Document!.Body!.InnerText;
    // Body is regenerated with project data — template placeholder lines are gone
    bodyText.Should().Contain("Proyecto de prueba");
    bodyText.Should().Contain("Contenido base");
    bodyText.Should().NotContain("{{PROJECT_TITLE}}");
    bodyText.Should().NotContain("{{PROJECT_ADDRESS}}");
    bodyText.Should().NotContain("Título: Proyecto Placeholder");
  }

  [Fact]
  public async Task ExportToDocxWithTemplateAsync_ShouldReplaceSplitRunPlaceholders_InHeader()
  {
    // Split-run placeholder replacement is important for headers/footers where Word may
    // split tokens like {{PROJECT_TITLE}} across multiple runs.
    var service = new DocxExportService();
    var data = CreateData("<p>Contenido base</p>") with
    {
      PlaceholderReplacements = new Dictionary<string, string>
      {
        ["PROJECT_TITLE"] = "Proyecto Split"
      }
    };

    var templateBytes = CreateDotxTemplateWithSplitPlaceholderInHeader();

    var bytes = await service.ExportToDocxWithTemplateAsync(data, templateBytes, CancellationToken.None);

    using var stream = new MemoryStream(bytes);
    using var doc = WordprocessingDocument.Open(stream, false);

    var headerText = doc.MainDocumentPart!.HeaderParts
      .FirstOrDefault()?.Header?.InnerText ?? string.Empty;
    headerText.Should().Contain("Proyecto Split");
    headerText.Should().NotContain("{{PROJECT_TITLE}}");
  }

    [Fact]
    public async Task ExportToDocxAsync_ShouldCreateNativeTable_WhenHtmlContainsTable()
    {
        var service = new DocxExportService();
        var data = CreateData("""
            <table>
              <thead>
                <tr><th>Normativa</th><th>Referencia</th></tr>
              </thead>
              <tbody>
                <tr><td>CTE DB-SI</td><td>Sección 1.1</td></tr>
              </tbody>
            </table>
            """);

        var bytes = await service.ExportToDocxAsync(data, CancellationToken.None);

        using var stream = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(stream, false);

        var mainPart = doc.MainDocumentPart;
        mainPart.Should().NotBeNull();
        mainPart!.Document.Should().NotBeNull();
        mainPart.Document!.Body.Should().NotBeNull();

        var table = mainPart.Document.Body!.Elements<Table>().FirstOrDefault();
        table.Should().NotBeNull();

        var rows = table!.Elements<TableRow>().ToList();
        rows.Count.Should().BeGreaterThanOrEqualTo(2);
        rows[0].Elements<TableCell>().Select(c => c.InnerText).Should().Contain(new[] { "Normativa", "Referencia" });
        rows[1].Elements<TableCell>().Select(c => c.InnerText).Should().Contain(new[] { "CTE DB-SI", "Sección 1.1" });
    }

    [Fact]
    public async Task ExportToDocxAsync_ShouldCreateClickableHyperlink_WhenHtmlContainsAnchor()
    {
        var service = new DocxExportService();
        var data = CreateData("<p>Consulta <a href=\"https://www.codigotecnico.org/\">CTE oficial</a></p>");

        var bytes = await service.ExportToDocxAsync(data, CancellationToken.None);

        using var stream = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(stream, false);

        var mainPart = doc.MainDocumentPart;
        mainPart.Should().NotBeNull();
        mainPart!.Document.Should().NotBeNull();

        var hyperlinks = mainPart.Document!.Descendants<Hyperlink>().ToList();
        hyperlinks.Should().ContainSingle();
        hyperlinks[0].InnerText.Should().Contain("CTE oficial");

        var hyperlinkId = hyperlinks[0].Id?.Value;
        hyperlinkId.Should().NotBeNullOrWhiteSpace();

        var relationship = mainPart.HyperlinkRelationships
            .FirstOrDefault(r => r.Id == hyperlinkId);

        relationship.Should().NotBeNull();
        relationship!.Uri.AbsoluteUri.Should().Be("https://www.codigotecnico.org/");
    }

      [Fact]
      public async Task ExportToDocxAsync_ShouldRenderInlineMathRun_WhenParagraphContainsInlineFormula()
      {
        var service = new DocxExportService();
        var data = CreateData("<p>Momento de inercia $I = b \\cdot h^3 / 12$ para verificación.</p>");

        var bytes = await service.ExportToDocxAsync(data, CancellationToken.None);

        using var stream = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(stream, false);

        var mainPart = doc.MainDocumentPart;
        mainPart.Should().NotBeNull();
        mainPart!.Document.Should().NotBeNull();

        var officeMath = mainPart.Document!.Descendants<DocMath.OfficeMath>().FirstOrDefault();

        officeMath.Should().NotBeNull();
        mainPart.Document.InnerText.Should().Contain("I = b \\cdot h^3 / 12");
        mainPart.Document.InnerText.Should().NotContain("$I = b \\cdot h^3 / 12$");
    }

    [Fact]
    public async Task ExportToDocxAsync_ShouldRenderCenteredParagraph_WhenContentIsBlockFormula()
    {
        var service = new DocxExportService();
        var data = CreateData("<p>$$E = mc^2$$</p>");

        var bytes = await service.ExportToDocxAsync(data, CancellationToken.None);

        using var stream = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(stream, false);

        var mainPart = doc.MainDocumentPart;
        mainPart.Should().NotBeNull();
        mainPart!.Document.Should().NotBeNull();

        var blockParagraph = mainPart.Document!.Descendants<Paragraph>()
            .FirstOrDefault(p =>
                p.InnerText.Contains("E = mc^2") &&
                p.ParagraphProperties?.Justification?.Val?.Value == JustificationValues.Center);

        blockParagraph.Should().NotBeNull();
        mainPart.Document!.Descendants<DocMath.OfficeMath>().Should().NotBeEmpty();
      }

      [Fact]
      public async Task ExportToDocxAsync_ShouldCreateFractionNode_ForLatexFractionFormula()
      {
        var service = new DocxExportService();
        var data = CreateData("<p>Flecha máxima $\\frac{qL^4}{8EI}$.</p>");

        var bytes = await service.ExportToDocxAsync(data, CancellationToken.None);

        using var stream = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(stream, false);

        var mainPart = doc.MainDocumentPart;
        mainPart.Should().NotBeNull();
        mainPart!.Document.Should().NotBeNull();

        var fractions = mainPart.Document!.Descendants<DocMath.Fraction>().ToList();

        fractions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExportToDocxAsync_ShouldCreateSummationWithLimits_ForLatexSummationFormula()
    {
        var service = new DocxExportService();
        var data = CreateData("<p>Combinación $\\sum_{i=1}^{n} q_i$.</p>");

        var bytes = await service.ExportToDocxAsync(data, CancellationToken.None);

        using var stream = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(stream, false);

        var mainPart = doc.MainDocumentPart;
        mainPart.Should().NotBeNull();
        mainPart!.Document.Should().NotBeNull();

        var subSup = mainPart.Document!.Descendants<DocMath.SubSuperscript>().FirstOrDefault();

        subSup.Should().NotBeNull();
        mainPart.Document.InnerText.Should().Contain("∑");
    }

    private static ExportDocumentData CreateData(string htmlContent)
    {
        var contentTreeJson = $$"""
        {
          "chapters": [
            {
              "id": "md-01",
              "title": "Cumplimiento Normativo",
              "content": {{System.Text.Json.JsonSerializer.Serialize(htmlContent)}},
              "sections": []
            }
          ]
        }
        """;

        return new ExportDocumentData(
            Title: "Proyecto de prueba",
            InterventionType: "Reforma",
            IsLoeRequired: false,
            ContentTreeJson: contentTreeJson,
            Address: "Calle Test 123");
    }

    private static byte[] CreateDotxTemplateWithTags(params string[] tags)
    {
      using var stream = new MemoryStream();

      using (var template = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Template, true))
      {
        var mainPart = template.AddMainDocumentPart();
        var body = new Body();

        foreach (var tag in tags)
        {
          var sdt = new SdtBlock(
            new SdtProperties(new Tag { Val = tag }),
            new SdtContentBlock(
              new Paragraph(
                new Run(new Text($"{{{{{tag}}}}}")))));

          body.AppendChild(sdt);
        }

        mainPart.Document = new Document(body);
        mainPart.Document.Save();
      }

      return stream.ToArray();
    }

    private static byte[] CreateDotxTemplateWithPlainText(params string[] lines)
    {
      using var stream = new MemoryStream();

      using (var template = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Template, true))
      {
        var mainPart = template.AddMainDocumentPart();
        var body = new Body();

        foreach (var line in lines)
        {
          body.AppendChild(new Paragraph(new Run(new Text(line))));
        }

        mainPart.Document = new Document(body);
        mainPart.Document.Save();
      }

      return stream.ToArray();
    }

    private static byte[] CreateDotxTemplateWithSplitPlaceholderInHeader()
    {
      using var stream = new MemoryStream();

      using (var template = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Template, true))
      {
        var mainPart = template.AddMainDocumentPart();

        // Header with {{PROJECT_TITLE}} split across three runs (as Word often serializes tokens)
        var headerPart = mainPart.AddNewPart<HeaderPart>();
        var headerParagraph = new Paragraph(
          new Run(new Text("{{PRO")),
          new Run(new Text("JECT_")),
          new Run(new Text("TITLE}}")));
        headerPart.Header = new Header(headerParagraph);
        headerPart.Header.Save();

        // Link header to section so it's associated with the document
        var body = new Body(
          new SectionProperties(
            new HeaderReference
            {
              Type = HeaderFooterValues.Default,
              Id = mainPart.GetIdOfPart(headerPart)
            }));
        mainPart.Document = new Document(body);
        mainPart.Document.Save();
      }

      return stream.ToArray();
    }
}
