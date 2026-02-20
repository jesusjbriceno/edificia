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
    /// <summary>
    /// Maps the DTO fields. <see cref="UserId"/> must be enriched
    /// from the JWT claim in the controller using a <c>with</c> expression.
    /// </summary>
    public static explicit operator UpdateProfileCommand(UpdateProfileRequest r)
        => new(Guid.Empty, r.FullName, r.CollegiateNumber);
}
