using Edificia.Application.Projects.Commands.CreateProject;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Edificia.API.Controllers;

/// <summary>
/// Controller for managing construction projects.
/// </summary>
public sealed class ProjectsController : BaseApiController
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Creates a new construction project.
    /// </summary>
    /// <param name="request">The project creation data.</param>
    /// <returns>The ID of the newly created project.</returns>
    /// <response code="201">Project created successfully.</response>
    /// <response code="400">Validation error in the request data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var command = new CreateProjectCommand(
            request.Title,
            request.InterventionType,
            request.IsLoeRequired,
            request.Description,
            request.Address,
            request.CadastralReference,
            request.LocalRegulations);

        var result = await _sender.Send(command);

        return HandleCreated(result, nameof(GetById), id => new { id });
    }

    /// <summary>
    /// Gets a project by its unique identifier.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <returns>The project details.</returns>
    /// <remarks>Placeholder — will be implemented in Feature 2.2 (project-read).</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id)
    {
        // Placeholder — GET by ID will be implemented with Dapper in Feature 2.2
        return Task.FromResult<IActionResult>(NotFound());
    }
}
