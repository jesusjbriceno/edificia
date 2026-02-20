using Edificia.Application.Users.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Users.Commands.UpdateUser;

/// <summary>
/// Command to update an existing user's profile and role.
/// UpdatedByUserId is injected from the JWT claim.
/// </summary>
public sealed record UpdateUserCommand(
    Guid UserId,
    string FullName,
    string Role,
    string? CollegiateNumber,
    Guid UpdatedByUserId) : IRequest<Result>
{
    /// <summary>
    /// Maps the DTO fields. <see cref="UserId"/> and <see cref="UpdatedByUserId"/> must be
    /// enriched from the route param and JWT claim in the controller using a <c>with</c> expression.
    /// </summary>
    public static explicit operator UpdateUserCommand(UpdateUserRequest r)
        => new(Guid.Empty, r.FullName, r.Role, r.CollegiateNumber, Guid.Empty);
}
