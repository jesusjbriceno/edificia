using System.Security.Claims;
using Edificia.Application.Auth.Commands.ChangePassword;
using Edificia.Application.Auth.Commands.Login;
using Edificia.Application.Auth.Commands.RefreshToken;
using Edificia.Application.Auth.Commands.RevokeToken;
using Edificia.Application.Auth.Commands.UpdateProfile;
using Edificia.Application.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Edificia.API.Controllers;

/// <summary>
/// Authentication endpoints: login and password management.
/// </summary>
public class AuthController : BaseApiController
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// If MustChangePassword is true, the token will have restricted access.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var command = (LoginCommand)request;
        var result = await _mediator.Send(command, ct);

        return HandleResult(result);
    }

    /// <summary>
    /// Changes the current user's password.
    /// This endpoint is accessible even when the user has a restricted token
    /// (MustChangePassword = true). On success, clears the restriction.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var command = (ChangePasswordCommand)request with { UserId = userId.Value };
        var result = await _mediator.Send(command, ct);

        return HandleResult(result);
    }

    /// <summary>
    /// Returns basic information about the currently authenticated user.
    /// Useful for frontend session validation.
    /// </summary>
    [HttpGet("me")]
    [Authorize(Policy = "ActiveUser")]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var response = new MeResponse(
            Id: userId.Value,
            Email: User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue("email"),
            FullName: User.FindFirstValue("full_name"),
            Roles: User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList().AsReadOnly());

        return Ok(response);
    }

    /// <summary>
    /// Updates the authenticated user's own profile (FullName and CollegiateNumber).
    /// </summary>
    [HttpPut("profile")]
    [Authorize(Policy = "ActiveUser")]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var command = (UpdateProfileCommand)request with { UserId = userId.Value };
        var result = await _mediator.Send(command, ct);

        return HandleResult(result);
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Implements token rotation: the old refresh token is revoked and a new pair is issued.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var command = (RefreshTokenCommand)request;
        var result = await _mediator.Send(command, ct);

        return HandleResult(result);
    }

    /// <summary>
    /// Revokes a refresh token (logout).
    /// The token will no longer be valid for obtaining new access tokens.
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Revoke(
        [FromBody] RevokeTokenRequest request,
        CancellationToken ct)
    {
        var command = (RevokeTokenCommand)request;
        var result = await _mediator.Send(command, ct);

        return HandleResult(result);
    }
}
