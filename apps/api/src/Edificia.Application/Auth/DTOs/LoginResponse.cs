namespace Edificia.Application.Auth.DTOs;

/// <summary>
/// Response DTO returned on successful authentication.
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    string? RefreshToken,
    int ExpiresInMinutes,
    bool MustChangePassword,
    UserInfo User);

/// <summary>
/// Basic user information included in login response.
/// </summary>
public sealed record UserInfo(
    Guid Id,
    string Email,
    string FullName,
    string? CollegiateNumber,
    IReadOnlyList<string> Roles);
