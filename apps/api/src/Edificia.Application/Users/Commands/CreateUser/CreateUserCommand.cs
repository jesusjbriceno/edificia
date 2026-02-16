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
    public static CreateUserCommand Create(Guid createdByUserId, CreateUserRequest r)
        => new(r.Email, r.FullName, r.Role, r.CollegiateNumber, createdByUserId);
}
