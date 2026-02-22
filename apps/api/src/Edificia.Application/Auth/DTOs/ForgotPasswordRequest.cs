namespace Edificia.Application.Auth.DTOs;

/// <summary>
/// Request DTO for the public forgot-password endpoint.
/// </summary>
public sealed record ForgotPasswordRequest(string Email);
