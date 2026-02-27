using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Edificia.Application.Interfaces;
using Edificia.Application.Templates;
using Edificia.Shared.Result;

namespace Edificia.Infrastructure.TemplateStorage;

public sealed class DotxTemplateFormatValidator : ITemplateFormatValidator
{
    private static readonly string[] MemoriaTecnicaRequiredTags =
    [
        "ProjectTitle",
        "MD.01",
        "MC.01"
    ];

    public Result Validate(string templateType, byte[] fileContent)
    {
        if (fileContent is null || fileContent.Length == 0)
        {
            return Result.Failure(TemplateErrors.InvalidFormat("el archivo está vacío."));
        }

        try
        {
            using var stream = new MemoryStream(fileContent, writable: false);
            using var document = WordprocessingDocument.Open(stream, false);

            if (document.MainDocumentPart?.Document?.Body is null)
            {
                return Result.Failure(TemplateErrors.InvalidFormat("la estructura Word no contiene Body principal."));
            }

            var tags = document.MainDocumentPart.Document
                .Descendants<Tag>()
                .Select(tag => tag.Val?.Value?.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToHashSet(StringComparer.Ordinal);

            if (tags.Count == 0)
            {
                return Result.Failure(TemplateErrors.InvalidFormat("no se han encontrado Content Controls con Tag."));
            }

            if (string.Equals(templateType, "MemoriaTecnica", StringComparison.OrdinalIgnoreCase))
            {
                var missing = MemoriaTecnicaRequiredTags
                    .Where(required => !tags.Contains(required))
                    .ToArray();

                if (missing.Length > 0)
                {
                    return Result.Failure(TemplateErrors.InvalidFormat(
                        $"faltan Tag(s) obligatorios para MemoriaTecnica: {string.Join(", ", missing)}."));
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(TemplateErrors.InvalidFormat(
                $"no se ha podido abrir el archivo como .dotx válido ({ex.Message})."));
        }
    }
}
