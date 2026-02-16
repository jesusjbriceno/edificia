namespace Edificia.Application.Auth.DTOs;

/// <summary>
/// Request DTO for revoking a refresh token (logout).
/// </summary>
public sealed record RevokeTokenRequest(string RefreshToken);
