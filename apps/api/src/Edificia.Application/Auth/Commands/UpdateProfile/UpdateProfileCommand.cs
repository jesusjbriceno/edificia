using Edificia.Application.Auth.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Auth.Commands.UpdateProfile;

/// <summary>
/// Command to update the authenticated user's own profile.
/// UserId is injected from the JWT claim (not from the request body).
/// </summary>
public sealed record UpdateProfileCommand(
    Guid UserId,
    string FullName,
    string? CollegiateNumber) : IRequest<Result<UpdateProfileResponse>>
{
    public static UpdateProfileCommand Create(Guid userId, UpdateProfileRequest r)
        => new(userId, r.FullName, r.CollegiateNumber);
}
