using FluentValidation;

namespace Edificia.Application.Auth.Commands.ChangePassword;

/// <summary>
/// FluentValidation validator for ChangePasswordCommand.
/// </summary>
public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El identificador de usuario es obligatorio.");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("La contraseña actual es obligatoria.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("La nueva contraseña es obligatoria.")
            .MinimumLength(8)
            .WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]")
            .WithMessage("La nueva contraseña debe contener al menos una letra mayúscula.")
            .Matches("[a-z]")
            .WithMessage("La nueva contraseña debe contener al menos una letra minúscula.")
            .Matches("[0-9]")
            .WithMessage("La nueva contraseña debe contener al menos un número.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("La nueva contraseña debe contener al menos un carácter especial.")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("La nueva contraseña no puede ser igual a la contraseña actual.");
    }
}
