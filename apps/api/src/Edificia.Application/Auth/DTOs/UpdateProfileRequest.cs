namespace Edificia.Application.Auth.DTOs;

/// <summary>
/// Request DTO for updating the authenticated user's own profile.
/// Only allows modifying FullName and CollegiateNumber.
/// </summary>
public sealed record UpdateProfileRequest(
    string FullName,
    string? CollegiateNumber);
