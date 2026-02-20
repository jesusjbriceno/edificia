using Edificia.Application.Auth.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Auth.Commands.ChangePassword;

/// <summary>
/// Command to change the current user's password.
/// UserId is injected from the JWT claim (not from the request body).
/// </summary>
public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : IRequest<Result>
{
    /// <summary>
    /// Maps the DTO fields. <see cref="UserId"/> must be enriched
    /// from the JWT claim in the controller using a <c>with</c> expression.
    /// </summary>
    public static explicit operator ChangePasswordCommand(ChangePasswordRequest r)
        => new(Guid.Empty, r.CurrentPassword, r.NewPassword);
}
