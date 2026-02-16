using Edificia.Application.Auth.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Auth.Commands.RevokeToken;

/// <summary>
/// Command to revoke a refresh token (logout).
/// </summary>
public sealed record RevokeTokenCommand(string Token) : IRequest<Result>
{
    public static explicit operator RevokeTokenCommand(RevokeTokenRequest r)
        => new(r.RefreshToken);
}
