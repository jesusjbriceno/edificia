namespace Edificia.Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant or business rule is violated.
/// </summary>
public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string code, string message)
        : base(code, message)
    {
    }
}
