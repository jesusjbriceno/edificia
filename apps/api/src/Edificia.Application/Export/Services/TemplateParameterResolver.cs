using Edificia.Application.Interfaces;
using Edificia.Domain.Constants;
using Edificia.Domain.Entities;

namespace Edificia.Application.Export.Services;

/// <summary>
/// Resolves global template placeholders from Project aggregate data.
/// </summary>
public sealed class TemplateParameterResolver : ITemplateParameterResolver
{
    public IReadOnlyDictionary<string, string> Resolve(
        Project project,
        IReadOnlyCollection<TemplateParam> activeParameters,
        DateTime? nowUtc = null)
    {
        var utcNow = nowUtc ?? DateTime.UtcNow;
        var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var parameter in activeParameters.Where(p => p.IsActive))
        {
            var value = ResolveSourceValue(project, parameter.SourceCode, utcNow);
            replacements[parameter.Key] = ApplyFormatter(value, parameter.Formatter);
        }

        return replacements;
    }

    private static string ResolveSourceValue(Project project, string sourceCode, DateTime utcNow)
    {
        return sourceCode.ToUpperInvariant() switch
        {
            TemplateParamSourceCodes.ProjectTitle => project.Title,
            TemplateParamSourceCodes.ProjectDescription => project.Description ?? string.Empty,
            TemplateParamSourceCodes.ProjectAddress => project.Address ?? string.Empty,
            TemplateParamSourceCodes.InterventionType => ExportDocumentData.FormatInterventionType(project.InterventionType),
            TemplateParamSourceCodes.IsLoeRequired => project.IsLoeRequired ? "SI" : "NO",
            TemplateParamSourceCodes.CadastralReference => project.CadastralReference ?? string.Empty,
            TemplateParamSourceCodes.LocalRegulations => project.LocalRegulations ?? string.Empty,
            TemplateParamSourceCodes.ExportDate => utcNow.ToString("dd/MM/yyyy"),
            TemplateParamSourceCodes.ExportDateTime => utcNow.ToString("dd/MM/yyyy HH:mm"),
            _ => string.Empty
        };
    }

    private static string ApplyFormatter(string value, string? formatter)
    {
        if (string.IsNullOrWhiteSpace(formatter))
        {
            return value;
        }

        return formatter.ToUpperInvariant() switch
        {
            TemplateParamFormatters.Upper => value.ToUpperInvariant(),
            TemplateParamFormatters.Lower => value.ToLowerInvariant(),
            TemplateParamFormatters.Trim => value.Trim(),
            _ => value
        };
    }
}
