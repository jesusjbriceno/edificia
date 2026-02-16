using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Users.Commands.ResetUserPassword;

/// <summary>
/// Command to reset a user's password to a new temporary password.
/// Sets MustChangePassword = true, sends reset email.
/// </summary>
public sealed record ResetUserPasswordCommand(
    Guid UserId,
    Guid PerformedByUserId) : IRequest<Result>;
