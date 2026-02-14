using Edificia.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Edificia.API.Middleware;

/// <summary>
/// Global exception handler implementing IExceptionHandler (.NET 8+).
/// Maps domain exceptions to RFC 7807 ProblemDetails responses.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var problemDetails = exception switch
        {
            EntityNotFoundException notFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Recurso no encontrado",
                Detail = notFound.Message,
                Type = "https://datatracker.ietf.org/doc/html/rfc7807",
                Extensions = { ["code"] = notFound.Code }
            },
            BusinessRuleException businessRule => new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Regla de negocio violada",
                Detail = businessRule.Message,
                Type = "https://datatracker.ietf.org/doc/html/rfc7807",
                Extensions = { ["code"] = businessRule.Code }
            },
            DomainException domainEx => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Error de dominio",
                Detail = domainEx.Message,
                Type = "https://datatracker.ietf.org/doc/html/rfc7807",
                Extensions = { ["code"] = domainEx.Code }
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor",
                Detail = "Ha ocurrido un error inesperado. Inténtelo de nuevo más tarde.",
                Type = "https://datatracker.ietf.org/doc/html/rfc7807"
            }
        };

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
