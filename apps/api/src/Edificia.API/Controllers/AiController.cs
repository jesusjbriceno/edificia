using Edificia.Application.Ai.Commands.GenerateSectionText;
using Edificia.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edificia.API.Controllers;

/// <summary>
/// Controller for AI text generation operations.
/// </summary>
[Authorize(Policy = AppPolicies.ActiveUser)]
public sealed class AiController : BaseApiController
{
    private readonly ISender _sender;

    public AiController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Generates AI-assisted text for a section of a project's content tree.
    /// Uses the Flux AI Gateway to produce content following CTE/LOE standards.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="request">The text generation request with prompt and optional context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated text response.</returns>
    /// <response code="200">Text generated successfully.</response>
    /// <response code="400">Validation error in the request data.</response>
    [HttpPost("/api/projects/{id:guid}/ai/generate")]
    [ProducesResponseType(typeof(GeneratedTextResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate(
        Guid id,
        [FromBody] GenerateTextRequest request,
        CancellationToken cancellationToken)
    {
        var command = GenerateSectionTextCommand.Create(id, request);
        var result = await _sender.Send(command, cancellationToken);

        return HandleResult(result);
    }
}
