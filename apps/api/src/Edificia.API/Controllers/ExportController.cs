using Edificia.Application.Export.Queries.ExportProject;
using Edificia.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edificia.API.Controllers;

/// <summary>
/// Controller for document export operations.
/// </summary>
[Route("api/projects")]
[Authorize(Policy = AppPolicies.ActiveUser)]
public sealed class ExportController : BaseApiController
{
    private readonly IMediator _mediator;

    public ExportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Exporta la memoria del proyecto como un documento Word (.docx).
    /// </summary>
    /// <param name="id">ID del proyecto a exportar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Archivo .docx descargable.</returns>
    /// <response code="200">Documento exportado correctamente.</response>
    /// <response code="404">No se encontró el proyecto.</response>
    /// <response code="400">El proyecto no tiene contenido para exportar.</response>
    [HttpGet("{id:guid}/export")]
    [Produces("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportProject(Guid id, CancellationToken cancellationToken)
    {
        var query = new ExportProjectQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return HandleResult(result);

        var response = result.Value;
        return File(response.FileContent, response.ContentType, response.FileName);
    }
}
