namespace Edificia.Application.Users.DTOs;

/// <summary>
/// Response DTO for user queries.
/// Maps 1:1 with the Dapper read model from asp_net_users table.
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    string? CollegiateNumber,
    bool IsActive,
    bool MustChangePassword,
    string Role,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
