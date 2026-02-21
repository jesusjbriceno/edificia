using Edificia.Application.Common;
using Edificia.Application.Projects.Commands.ApproveProject;
using Edificia.Application.Projects.Commands.CreateProject;
using Edificia.Application.Projects.Commands.PatchSectionContent;
using Edificia.Application.Projects.Commands.RejectProject;
using Edificia.Application.Projects.Commands.SubmitForReview;
using Edificia.Application.Projects.Commands.UpdateProjectTree;
using Edificia.Application.Projects.Queries;
using Edificia.Application.Projects.Queries.GetProjectById;
using Edificia.Application.Projects.Queries.GetProjectTree;
using Edificia.Application.Projects.Queries.GetProjects;
using Edificia.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edificia.API.Controllers;

/// <summary>
/// Controller for managing construction projects.
/// </summary>
[Authorize(Policy = AppPolicies.ActiveUser)]
public sealed class ProjectsController : BaseApiController
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves a paginated list of projects.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (1-50, default: 10).</param>
    /// <param name="status">Optional filter by status (Draft, InProgress, Completed, Archived).</param>
    /// <param name="search">Optional search term (matches title or description).</param>
    /// <response code="200">Returns the paginated list of projects.</response>
    /// <response code="400">Invalid pagination or filter parameters.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        var query = new GetProjectsQuery(page, pageSize, status, search);
        var result = await _sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Gets a project by its unique identifier.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <returns>The project details.</returns>
    /// <response code="200">Returns the project.</response>
    /// <response code="404">Project not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProjectByIdQuery(id);
        var result = await _sender.Send(query);

        return HandleResult(result);
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
        var command = (CreateProjectCommand)request;
        var result = await _sender.Send(command);

        return HandleCreated(result, nameof(GetById), id => new { id });
    }

    /// <summary>
    /// Gets the normative content tree of a project.
    /// Returns the raw JSONB tree along with intervention metadata for client-side filtering.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <returns>The content tree JSON and intervention metadata.</returns>
    /// <response code="200">Returns the content tree.</response>
    /// <response code="404">Project not found.</response>
    [HttpGet("{id:guid}/tree")]
    [ProducesResponseType(typeof(ContentTreeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTree(Guid id)
    {
        var query = new GetProjectTreeQuery(id);
        var result = await _sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Updates the normative content tree of a project.
    /// Replaces the entire content tree JSON (JSONB column).
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="request">The content tree data.</param>
    /// <response code="204">Content tree updated successfully.</response>
    /// <response code="400">Validation error in the request data.</response>
    /// <response code="404">Project not found.</response>
    [HttpPut("{id:guid}/tree")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTree(Guid id, [FromBody] UpdateProjectTreeRequest request)
    {
        var command = UpdateProjectTreeCommand.Create(id, request);
        var result = await _sender.Send(command);

        return HandleNoContent(result);
    }

    /// <summary>
    /// Updates the content of a specific section within the project's content tree.
    /// Performs a partial update on a single section (JSONB path update).
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="sectionId">The section ID within the content tree.</param>
    /// <param name="request">The section content data.</param>
    /// <response code="204">Section content updated successfully.</response>
    /// <response code="400">Validation error in the request data.</response>
    /// <response code="404">Project or section not found.</response>
    [HttpPatch("{id:guid}/sections/{sectionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchSectionContent(
        Guid id, string sectionId, [FromBody] UpdateSectionRequest request)
    {
        var command = PatchSectionContentCommand.Create(id, sectionId, request);
        var result = await _sender.Send(command);

        return HandleNoContent(result);
    }

    /// <summary>
    /// Submits a project for review. Transitions status to PendingReview.
    /// Available from Draft or InProgress states.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <response code="204">Project submitted for review successfully.</response>
    /// <response code="404">Project not found.</response>
    /// <response code="422">Invalid state transition.</response>
    [HttpPost("{id:guid}/submit-review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SubmitForReview(Guid id)
    {
        var command = new SubmitForReviewCommand(id);
        var result = await _sender.Send(command);

        return HandleNoContent(result);
    }

    /// <summary>
    /// Approves a project under review. Transitions status to Completed.
    /// Only available from PendingReview state. Requires Admin or Root role.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <response code="204">Project approved successfully.</response>
    /// <response code="404">Project not found.</response>
    /// <response code="422">Invalid state transition.</response>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ApproveProject(Guid id)
    {
        var command = new ApproveProjectCommand(id);
        var result = await _sender.Send(command);

        return HandleNoContent(result);
    }

    /// <summary>
    /// Rejects a project under review. Transitions status back to Draft.
    /// Only available from PendingReview state. Requires Admin or Root role.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="request">The rejection reason.</param>
    /// <response code="204">Project rejected successfully.</response>
    /// <response code="400">Validation error (reason required).</response>
    /// <response code="404">Project not found.</response>
    /// <response code="422">Invalid state transition.</response>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RejectProject(Guid id, [FromBody] RejectProjectRequest request)
    {
        var command = new RejectProjectCommand(id, request.Reason);
        var result = await _sender.Send(command);

        return HandleNoContent(result);
    }
}
