using System.Security.Claims;
using Edificia.Application.Templates.Commands.CreateTemplate;
using Edificia.Application.Templates.Commands.ToggleTemplateStatus;
using Edificia.Application.Templates.DTOs;
using Edificia.Application.Templates.Queries.GetTemplates;
using Edificia.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edificia.API.Controllers;

[Authorize(Policy = AppPolicies.RequireAdmin)]
public sealed class TemplatesController : BaseApiController
{
    private readonly ISender _sender;

    public TemplatesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromForm] string name,
        [FromForm] string templateType,
        [FromForm] string? description,
        [FromForm] IFormFile templateFile,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null) return Unauthorized();

        byte[] fileBytes;
        await using (var stream = new MemoryStream())
        {
            await templateFile.CopyToAsync(stream, cancellationToken);
            fileBytes = stream.ToArray();
        }

        var request = new CreateTemplateRequest(name, templateType, description);
        var command = CreateTemplateCommand.Create(
            currentUserId.Value,
            request,
            templateFile.FileName,
            string.IsNullOrWhiteSpace(templateFile.ContentType)
                ? "application/octet-stream"
                : templateFile.ContentType,
            templateFile.Length,
            fileBytes);

        var result = await _sender.Send(command, cancellationToken);
        return HandleCreated(result, nameof(GetAll), _ => new { });
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? templateType = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTemplatesQuery(templateType, isActive);
        var result = await _sender.Send(query, cancellationToken);

        return HandleResult(result);
    }

    [HttpPut("{id:guid}/toggle-status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleStatus(
        Guid id,
        [FromBody] ToggleTemplateStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = ToggleTemplateStatusCommand.Create(id, request);
        var result = await _sender.Send(command, cancellationToken);

        return HandleNoContent(result);
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
