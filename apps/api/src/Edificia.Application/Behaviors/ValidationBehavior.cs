using Edificia.Shared.Result;
using FluentValidation;
using MediatR;

namespace Edificia.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators before the handler.
/// Returns a ValidationResult with errors instead of throwing exceptions.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Length != 0)
            return CreateValidationResult<TResponse>(errors);

        return await next(cancellationToken);
    }

    private static TResponse CreateValidationResult<TResult>(Error[] errors)
    {
        // For Result (non-generic)
        if (typeof(TResult) == typeof(Result))
            return (TResponse)(object)Result.ValidationFailure(errors);

        // For Result<T>
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var validationResultType = typeof(ValidationResult<>).MakeGenericType(resultType);

        var validationResult = validationResultType
            .GetMethod(nameof(ValidationResult<object>.WithErrors))!
            .Invoke(null, [errors])!;

        return (TResponse)validationResult;
    }
}
