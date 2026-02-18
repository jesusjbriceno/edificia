using Edificia.Domain.Entities;

namespace Edificia.Application.Auth.DTOs;

/// <summary>
/// Response DTO returned after a successful profile update.
/// </summary>
public sealed record UpdateProfileResponse(
    Guid Id,
    string Email,
    string FullName,
    string? CollegiateNumber)
{
    /// <summary>
    /// Creates an UpdateProfileResponse from an ApplicationUser entity.
    /// </summary>
    public static UpdateProfileResponse FromUser(ApplicationUser user) => new(
        Id: user.Id,
        Email: user.Email!,
        FullName: user.FullName,
        CollegiateNumber: user.CollegiateNumber);
}
