using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Public (anonymous) command to request a password reset.
/// Generates a reset token and sends an email with instructions.
/// Always returns success to prevent email enumeration.
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;
