using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Users.Commands.DeleteUser;

/// <summary>
/// Command to delete a user from the system.
/// Enforces role hierarchy: Admin cannot delete Root/Admin users, cannot delete self.
/// </summary>
public sealed record DeleteUserCommand(
    Guid UserId,
    Guid PerformedByUserId) : IRequest<Result>;
