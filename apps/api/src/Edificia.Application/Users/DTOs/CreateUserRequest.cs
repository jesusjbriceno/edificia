namespace Edificia.Application.Users.DTOs;

/// <summary>
/// Request DTO for creating a new user.
/// </summary>
public sealed record CreateUserRequest(
    string Email,
    string FullName,
    string Role,
    string? CollegiateNumber = null);
