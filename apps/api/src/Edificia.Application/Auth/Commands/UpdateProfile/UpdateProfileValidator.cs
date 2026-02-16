using FluentValidation;

namespace Edificia.Application.Auth.Commands.UpdateProfile;

/// <summary>
/// FluentValidation validator for UpdateProfileCommand.
/// </summary>
public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El identificador de usuario es obligatorio.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(200)
            .WithMessage("El nombre completo no puede exceder 200 caracteres.");

        RuleFor(x => x.CollegiateNumber)
            .MaximumLength(50)
            .WithMessage("El número de colegiado no puede exceder 50 caracteres.")
            .When(x => x.CollegiateNumber is not null);
    }
}
