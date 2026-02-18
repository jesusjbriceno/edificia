using Edificia.Domain.Entities;

namespace Edificia.Application.Auth.DTOs;

/// <summary>
/// Response DTO returned on successful authentication.
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    string? RefreshToken,
    int ExpiresInMinutes,
    bool MustChangePassword,
    UserInfo User)
{
    /// <summary>
    /// Creates a LoginResponse from an ApplicationUser, tokens and roles.
    /// Centralises the mapping that was previously duplicated in Login and RefreshToken handlers.
    /// </summary>
    public static LoginResponse FromUser(
        ApplicationUser user,
        string accessToken,
        string refreshToken,
        int expiresInMinutes,
        IList<string> roles) => new(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresInMinutes: expiresInMinutes,
            MustChangePassword: user.MustChangePassword,
            User: new UserInfo(
                Id: user.Id,
                Email: user.Email!,
                FullName: user.FullName,
                CollegiateNumber: user.CollegiateNumber,
                Roles: roles.ToList().AsReadOnly()));
}

/// <summary>
/// Basic user information included in login response.
/// </summary>
public sealed record UserInfo(
    Guid Id,
    string Email,
    string FullName,
    string? CollegiateNumber,
    IReadOnlyList<string> Roles);
