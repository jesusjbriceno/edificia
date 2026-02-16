using Edificia.Application.Auth.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user and obtain a JWT token.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<Result<LoginResponse>>
{
    public static explicit operator LoginCommand(LoginRequest r)
        => new(r.Email, r.Password);
}
