namespace Edificia.Application.Auth.DTOs;

/// <summary>
/// Request DTO for user login.
/// </summary>
public sealed record LoginRequest(
    string Email,
    string Password);
