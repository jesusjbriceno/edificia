using System.Diagnostics;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs the execution of every command and query.
/// Captures the request type, elapsed time, and outcome (success/failure).
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "Handling {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            if (response is Result result && result.IsFailure)
            {
                _logger.LogWarning(
                    "Handled {RequestName} with failure: {ErrorCode} — {ErrorDescription} ({ElapsedMs}ms)",
                    requestName, result.Error.Code, result.Error.Description,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "Handled {RequestName} successfully ({ElapsedMs}ms)",
                    requestName, stopwatch.ElapsedMilliseconds);
            }

            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning(
                    "Long running request: {RequestName} ({ElapsedMs}ms)",
                    requestName, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "Unhandled exception in {RequestName} ({ElapsedMs}ms)",
                requestName, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
