namespace Edificia.Shared.Result;

/// <summary>
/// Specialized result for FluentValidation failures containing multiple errors.
/// </summary>
public sealed class ValidationResult : Result
{
    public Error[] Errors { get; }

    internal ValidationResult(Error[] errors)
        : base(false, errors.Length > 0 ? errors[0] : Error.Validation("Unknown", "Validation failed."))
    {
        Errors = errors;
    }
}

/// <summary>
/// Typed validation result for generic handlers.
/// </summary>
public sealed class ValidationResult<T> : Result<T>
{
    public Error[] Errors { get; }

    internal ValidationResult(Error[] errors)
        : base(default, false, errors.Length > 0 ? errors[0] : Error.Validation("Unknown", "Validation failed."))
    {
        Errors = errors;
    }

    public static ValidationResult<T> WithErrors(Error[] errors) => new(errors);
}
