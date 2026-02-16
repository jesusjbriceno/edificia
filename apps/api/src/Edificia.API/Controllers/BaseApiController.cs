using Edificia.Shared.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Edificia.API.Controllers;

/// <summary>
/// Base API controller that translates Result&lt;T&gt; to proper HTTP responses.
/// All controllers must inherit from this.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Converts a Result to an IActionResult with proper status codes.
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok();

        return HandleError(result);
    }

    /// <summary>
    /// Converts a Result to a 204 No Content response on success.
    /// Use for PUT/PATCH operations that don't return a body.
    /// </summary>
    protected IActionResult HandleNoContent(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return HandleError(result);
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to an IActionResult with the value or error.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return HandleError(result);
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to a 201 Created response.
    /// </summary>
    protected IActionResult HandleCreated<T>(Result<T> result, string actionName, Func<T, object> routeValues)
    {
        if (result.IsSuccess)
            return CreatedAtAction(actionName, routeValues(result.Value), result.Value);

        return HandleError(result);
    }

    private IActionResult HandleError(Result result)
    {
        if (result is ValidationResult validationResult)
        {
            var errors = validationResult.Errors
                .Select(e => new { code = e.Code, description = e.Description })
                .ToArray();

            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Error de validación",
                Type = "https://datatracker.ietf.org/doc/html/rfc7807",
                Extensions = { ["errors"] = errors }
            });
        }

        var error = result.Error;

        return error.Code switch
        {
            var code when code.StartsWith("NotFound") => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Recurso no encontrado",
                Detail = error.Description,
                Extensions = { ["code"] = error.Code }
            }),
            var code when code.StartsWith("Conflict") => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflicto",
                Detail = error.Description,
                Extensions = { ["code"] = error.Code }
            }),
            var code when code.StartsWith("Unauthorized") => Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "No autorizado",
                Detail = error.Description,
                Extensions = { ["code"] = error.Code }
            }),
            var code when code.StartsWith("Forbidden") => new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Prohibido",
                Detail = error.Description,
                Extensions = { ["code"] = error.Code }
            })
            { StatusCode = StatusCodes.Status403Forbidden },
            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Error en la solicitud",
                Detail = error.Description,
                Extensions = { ["code"] = error.Code }
            })
        };
    }
}
