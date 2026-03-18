using Edificia.Domain.Entities;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Resolves placeholder replacements to be applied during template-based export.
/// </summary>
public interface ITemplatePlaceholderService
{
    Task<IReadOnlyDictionary<string, string>> ResolveAsync(
        Project project,
        CancellationToken cancellationToken = default);
}
