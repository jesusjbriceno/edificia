namespace Edificia.Application.Auth.DTOs;

/// <summary>
/// Response DTO returned after a successful profile update.
/// </summary>
public sealed record UpdateProfileResponse(
    Guid Id,
    string Email,
    string FullName,
    string? CollegiateNumber);
