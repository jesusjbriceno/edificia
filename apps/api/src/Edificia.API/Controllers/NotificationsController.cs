using System.Security.Claims;
using Edificia.Application.Notifications.Commands.MarkAllAsRead;
using Edificia.Application.Notifications.Commands.MarkAsRead;
using Edificia.Application.Notifications.Queries.GetNotifications;
using Edificia.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edificia.API.Controllers;

/// <summary>
/// Controller for managing user notifications.
/// </summary>
[Authorize(Policy = AppPolicies.ActiveUser)]
public sealed class NotificationsController : BaseApiController
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves notifications for the current authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var query = new GetNotificationsQuery(userId);
        var result = await _sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var command = new MarkAsReadCommand(id);
        var result = await _sender.Send(command);

        return HandleNoContent(result);
    }

    /// <summary>
    /// Marks all notifications for the current user as read.
    /// </summary>
    [HttpPost("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllRead()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var command = new MarkAllAsReadCommand(userId);
        var result = await _sender.Send(command);

        return HandleNoContent(result);
    }
}
