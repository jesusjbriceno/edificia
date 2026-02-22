using Edificia.Application.Users.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Users.Commands.CreateUser;

/// <summary>
/// Command to create a new user with a temporary password.
/// The user will be required to change their password on first login.
/// </summary>
public sealed record CreateUserCommand(
    string Email,
    string FullName,
    string Role,
    string? CollegiateNumber,
    Guid CreatedByUserId) : IRequest<Result<Guid>>
{
    /// <summary>
    /// Maps the DTO fields. <see cref="CreatedByUserId"/> must be enriched
    /// from the JWT claim in the controller using a <c>with</c> expression.
    /// </summary>
    public static explicit operator CreateUserCommand(CreateUserRequest r)
        => new(r.Email, r.FullName, r.Role, r.CollegiateNumber, Guid.Empty);
}
