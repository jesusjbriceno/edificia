using System.IO;
using DocMath = DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Edificia.Application.Interfaces;
using Edificia.Infrastructure.Export;
using FluentAssertions;

namespace Edificia.Application.Tests.Export;

public class DocxExportServiceTests
{
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
}
