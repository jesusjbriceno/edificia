using Edificia.Domain.Entities;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Resolves active template parameters into key-value replacements.
/// </summary>
public interface ITemplateParameterResolver
{
    IReadOnlyDictionary<string, string> Resolve(
        Project project,
        IReadOnlyCollection<TemplateParam> activeParameters,
        DateTime? nowUtc = null);
}
