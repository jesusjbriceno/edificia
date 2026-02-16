namespace Edificia.Application.Users.DTOs;

/// <summary>
/// Request DTO for updating an existing user.
/// </summary>
public sealed record UpdateUserRequest(
    string FullName,
    string Role,
    string? CollegiateNumber = null);
