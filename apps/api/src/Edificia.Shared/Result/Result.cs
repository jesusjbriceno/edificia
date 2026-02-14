namespace Edificia.Shared.Result;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// Uses static factory methods instead of throwing exceptions for control flow.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, Error.None);

    public static Result<T> Failure<T>(Error error) => new(default, false, error);

    public static Result<T> NotFound<T>(Error error) => new(default, false, error);

    /// <summary>
    /// Creates a validation failure result with multiple errors.
    /// </summary>
    public static ValidationResult ValidationFailure(Error[] errors)
        => new(errors);
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type T.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    public static implicit operator Result<T>(T value) => Success(value);
}
