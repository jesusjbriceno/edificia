namespace Edificia.Application.Auth.DTOs;

/// <summary>
/// Response DTO for the /auth/me endpoint.
/// Returns basic information about the currently authenticated user.
/// </summary>
public sealed record MeResponse(
    Guid Id,
    string? Email,
    string? FullName,
    IReadOnlyList<string> Roles);
