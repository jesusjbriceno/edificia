namespace Edificia.Shared.Result;

/// <summary>
/// Represents a typed error with a code and description.
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Validation(string code, string description)
        => new($"Validation.{code}", description);

    public static Error NotFound(string code, string description)
        => new($"NotFound.{code}", description);

    public static Error Conflict(string code, string description)
        => new($"Conflict.{code}", description);

    public static Error Failure(string code, string description)
        => new($"Failure.{code}", description);

    public static Error Unauthorized(string code, string description)
        => new($"Unauthorized.{code}", description);

    public static Error Forbidden(string code, string description)
        => new($"Forbidden.{code}", description);
}
