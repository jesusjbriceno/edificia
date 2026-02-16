using FluentValidation;

namespace Edificia.Application.Auth.Commands.Login;

/// <summary>
/// FluentValidation validator for LoginCommand.
/// </summary>
public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es obligatorio.")
            .EmailAddress()
            .WithMessage("El formato del email no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria.");
    }
}
