namespace Edificia.Application.Auth.DTOs;

/// <summary>
/// Request DTO for changing the user's password.
/// Used both for forced change (MustChangePassword) and voluntary change.
/// </summary>
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
