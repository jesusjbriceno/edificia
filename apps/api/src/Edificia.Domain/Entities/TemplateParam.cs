using Edificia.Domain.Constants;
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

        var normalizedKey = key.Trim().ToUpperInvariant();
        var normalizedSourceCode = sourceCode.Trim().ToUpperInvariant();
        var normalizedFormatter = Normalize(formatter);

        EnsureValidKey(normalizedKey);
        EnsureSupportedSourceCode(normalizedSourceCode);
        EnsureSupportedFormatter(normalizedFormatter);

        return new TemplateParam(Guid.NewGuid())
        {
            Key = normalizedKey,
            DisplayName = displayName.Trim(),
            SourceCode = normalizedSourceCode,
            Formatter = normalizedFormatter,
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

        var normalizedSourceCode = sourceCode.Trim().ToUpperInvariant();
        var normalizedFormatter = Normalize(formatter);

        EnsureSupportedSourceCode(normalizedSourceCode);
        EnsureSupportedFormatter(normalizedFormatter);

        DisplayName = displayName.Trim();
        SourceCode = normalizedSourceCode;
        Formatter = normalizedFormatter;
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

    private static void EnsureValidKey(string key)
    {
        var isValid = key.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch == '_');
        if (isValid)
        {
            return;
        }

        throw new BusinessRuleException(
            "TemplateParam.InvalidKeyFormat",
            "La clave del parámetro solo puede contener letras, números y guion bajo (_)."
        );
    }

    private static void EnsureSupportedSourceCode(string sourceCode)
    {
        if (TemplateParamSourceCodes.IsSupported(sourceCode))
        {
            return;
        }

        throw new BusinessRuleException(
            "TemplateParam.UnsupportedSourceCode",
            $"El source code '{sourceCode}' no está soportado por el catálogo global."
        );
    }

    private static void EnsureSupportedFormatter(string? formatter)
    {
        if (TemplateParamFormatters.IsSupported(formatter))
        {
            return;
        }

        throw new BusinessRuleException(
            "TemplateParam.UnsupportedFormatter",
            $"El formatter '{formatter}' no está soportado."
        );
    }
}
