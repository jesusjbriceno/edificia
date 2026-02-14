namespace Edificia.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-level errors.
/// These represent truly exceptional situations (not control flow).
/// </summary>
public abstract class DomainException : Exception
{
    public string Code { get; }

    protected DomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    protected DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}
