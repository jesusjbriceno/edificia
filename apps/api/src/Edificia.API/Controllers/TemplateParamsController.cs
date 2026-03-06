using Edificia.Application.TemplateParams.Commands.SetTemplateParamActivation;
using Edificia.Application.TemplateParams.DTOs;
using Edificia.Application.TemplateParams.Queries.GetTemplateParams;
using Edificia.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Edificia.API.Controllers;

[Authorize(Policy = AppPolicies.RequireAdmin)]
[Route("api/template-params")]
public sealed class TemplateParamsController : BaseApiController
{
    private readonly ISender _sender;

    public TemplateParamsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TemplateParamResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTemplateParamsQuery(isActive);
        var result = await _sender.Send(query, cancellationToken);

        return HandleResult(result);
    }

    [HttpPut("{id:guid}/activation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActivation(
        Guid id,
        [FromBody] SetTemplateParamActivationRequest request,
        CancellationToken cancellationToken)
    {
        var command = SetTemplateParamActivationCommand.Create(id, request);
        var result = await _sender.Send(command, cancellationToken);

        return HandleNoContent(result);
    }
}
