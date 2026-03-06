using Edificia.Domain.Exceptions;
using Edificia.Domain.Primitives;

namespace Edificia.Domain.Entities;

/// <summary>
/// Global parameter definition used to resolve template placeholders.
/// </summary>
public sealed class TemplateParam : AuditableEntity
{
    public string Key { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string SourceCode { get; private set; } = string.Empty;
    public string? Formatter { get; private set; }
    public bool IsActive { get; private set; }

    private TemplateParam() { }

    private TemplateParam(Guid id) : base(id) { }

    public static TemplateParam Create(
        string key,
        string displayName,
        string sourceCode,
        string? formatter = null,
        bool isActive = true)
    {
        EnsureRequired(key, nameof(key));
        EnsureRequired(displayName, nameof(displayName));
        EnsureRequired(sourceCode, nameof(sourceCode));

        return new TemplateParam(Guid.NewGuid())
        {
            Key = key.Trim().ToUpperInvariant(),
            DisplayName = displayName.Trim(),
            SourceCode = sourceCode.Trim().ToUpperInvariant(),
            Formatter = Normalize(formatter),
            IsActive = isActive
        };
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void UpdateMetadata(string displayName, string sourceCode, string? formatter)
    {
        EnsureRequired(displayName, nameof(displayName));
        EnsureRequired(sourceCode, nameof(sourceCode));

        DisplayName = displayName.Trim();
        SourceCode = sourceCode.Trim().ToUpperInvariant();
        Formatter = Normalize(formatter);
    }

    private static void EnsureRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessRuleException(
                "TemplateParam.InvalidField",
                $"El campo '{fieldName}' es obligatorio.");
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToUpperInvariant();
    }
}
