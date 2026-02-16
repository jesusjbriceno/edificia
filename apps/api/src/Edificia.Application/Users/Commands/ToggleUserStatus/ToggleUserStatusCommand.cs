using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Users.Commands.ToggleUserStatus;

/// <summary>
/// Command to activate or deactivate a user.
/// Activate = true sets IsActive to true; Activate = false sets it to false.
/// </summary>
public sealed record ToggleUserStatusCommand(
    Guid UserId,
    bool Activate,
    Guid PerformedByUserId) : IRequest<Result>;
