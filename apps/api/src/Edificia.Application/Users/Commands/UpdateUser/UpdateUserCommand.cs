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
    public static UpdateUserCommand Create(Guid userId, Guid updatedByUserId, UpdateUserRequest r)
        => new(userId, r.FullName, r.Role, r.CollegiateNumber, updatedByUserId);
}
