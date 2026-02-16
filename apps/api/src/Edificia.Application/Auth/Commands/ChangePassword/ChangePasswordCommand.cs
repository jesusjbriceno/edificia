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
    string NewPassword) : IRequest<Result>;
