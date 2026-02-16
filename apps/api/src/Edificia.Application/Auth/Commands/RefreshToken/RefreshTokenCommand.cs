using Edificia.Application.Auth.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh an expired access token using a valid refresh token.
/// Implements token rotation: old refresh token is revoked, new pair is issued.
/// </summary>
public sealed record RefreshTokenCommand(string Token) : IRequest<Result<LoginResponse>>
{
    public static explicit operator RefreshTokenCommand(RefreshTokenRequest r)
        => new(r.RefreshToken);
}
