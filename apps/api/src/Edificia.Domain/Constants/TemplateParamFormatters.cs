namespace Edificia.Domain.Constants;

/// <summary>
/// Supported formatters for template parameter values.
/// </summary>
public static class TemplateParamFormatters
{
    public const string Upper = "UPPER";
    public const string Lower = "LOWER";
    public const string Trim = "TRIM";

    public static readonly string[] All =
    [
        Upper,
        Lower,
        Trim
    ];

    private static readonly HashSet<string> AllSet =
    [
        ..All
    ];

    public static bool IsSupported(string? formatter)
    {
        if (string.IsNullOrWhiteSpace(formatter))
        {
            return true;
        }

        return AllSet.Contains(formatter.Trim().ToUpperInvariant());
    }
}
