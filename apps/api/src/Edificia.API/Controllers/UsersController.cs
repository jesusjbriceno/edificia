using System.Security.Claims;
using Edificia.Application.Common;
using Edificia.Application.Users.Commands.CreateUser;
using Edificia.Application.Users.Commands.DeleteUser;
using Edificia.Application.Users.Commands.ResetUserPassword;
using Edificia.Application.Users.Commands.ToggleUserStatus;
using Edificia.Application.Users.Commands.UpdateUser;
using Edificia.Application.Users.DTOs;
using Edificia.Application.Users.Queries.GetUserById;
using Edificia.Application.Users.Queries.GetUsers;
using Edificia.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Edificia.API.Controllers;

/// <summary>
/// Controller for user management (CRUD).
/// All endpoints require Admin or Root role.
/// </summary>
[Authorize(Policy = AppPolicies.RequireAdmin)]
public sealed class UsersController : BaseApiController
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves a paginated list of users with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null)
    {
        var query = new GetUsersQuery(page, pageSize, role, isActive, search);
        var result = await _sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves a single user by their ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new user with a temporary password.
    /// The user will be required to change their password on first login.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null) return Unauthorized();

        var command = CreateUserCommand.Create(currentUserId.Value, request);
        var result = await _sender.Send(command, ct);

        return HandleCreated(result, nameof(GetById), id => new { id });
    }

    /// <summary>
    /// Updates an existing user's profile and role.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null) return Unauthorized();

        var command = UpdateUserCommand.Create(id, currentUserId.Value, request);
        var result = await _sender.Send(command, ct);

        return HandleNoContent(result);
    }

    /// <summary>
    /// Deactivates a user. Login will be blocked.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null) return Unauthorized();

        var command = new ToggleUserStatusCommand(id, Activate: false, currentUserId.Value);
        var result = await _sender.Send(command, ct);

        return HandleResult(result);
    }

    /// <summary>
    /// Reactivates a previously deactivated user.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null) return Unauthorized();

        var command = new ToggleUserStatusCommand(id, Activate: true, currentUserId.Value);
        var result = await _sender.Send(command, ct);

        return HandleResult(result);
    }

    /// <summary>
    /// Resets a user's password to a new temporary password.
    /// The user will be required to change their password on next login.
    /// </summary>
    [HttpPost("{id:guid}/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null) return Unauthorized();

        var command = new ResetUserPasswordCommand(id, currentUserId.Value);
        var result = await _sender.Send(command, ct);

        return HandleResult(result);
    }

    /// <summary>
    /// Deletes a user from the system permanently.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null) return Unauthorized();

        var command = new DeleteUserCommand(id, currentUserId.Value);
        var result = await _sender.Send(command, ct);

        return HandleNoContent(result);
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
