namespace Edificia.Application.Auth.DTOs;

/// <summary>
/// Request DTO for refreshing an access token.
/// </summary>
public sealed record RefreshTokenRequest(string RefreshToken);
