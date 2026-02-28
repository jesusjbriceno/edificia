using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Edificia.Application.Interfaces;
using Edificia.Application.Templates;
using Edificia.Shared.Result;

namespace Edificia.Infrastructure.TemplateStorage;

public sealed class DotxTemplateFormatValidator : ITemplateFormatValidator
{
    private static readonly byte[][] ZipSignatures =
    [
        [0x50, 0x4B, 0x03, 0x04],
        [0x50, 0x4B, 0x05, 0x06],
        [0x50, 0x4B, 0x07, 0x08]
    ];

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

        if (!LooksLikeZip(fileContent))
        {
            return Result.Failure(TemplateErrors.InvalidFormat("el archivo no tiene firma ZIP/OpenXML válida para .dotx."));
        }

        try
        {
            using var stream = new MemoryStream(fileContent, writable: false);
            using var document = WordprocessingDocument.Open(stream, false);

            if (document.DocumentType != WordprocessingDocumentType.Template)
            {
                return Result.Failure(TemplateErrors.InvalidFormat("el documento no es una plantilla .dotx."));
            }

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

    private static bool LooksLikeZip(byte[] content)
    {
        if (content.Length < 4)
        {
            return false;
        }

        return ZipSignatures.Any(signature =>
            content[0] == signature[0]
            && content[1] == signature[1]
            && content[2] == signature[2]
            && content[3] == signature[3]);
    }
}
