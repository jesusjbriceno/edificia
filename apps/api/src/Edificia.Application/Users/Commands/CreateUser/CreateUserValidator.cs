using Edificia.Domain.Constants;
using FluentValidation;

namespace Edificia.Application.Users.Commands.CreateUser;

/// <summary>
/// FluentValidation validator for CreateUserCommand.
/// </summary>
public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    private static readonly string[] AllowedRoles =
        [AppRoles.Admin, AppRoles.Architect, AppRoles.Collaborator];

    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es obligatorio.")
            .EmailAddress()
            .WithMessage("El formato del email no es válido.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(200)
            .WithMessage("El nombre completo no puede superar los 200 caracteres.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("El rol es obligatorio.")
            .Must(r => AllowedRoles.Contains(r))
            .WithMessage($"El rol debe ser uno de: {string.Join(", ", AllowedRoles)}.");

        RuleFor(x => x.CollegiateNumber)
            .MaximumLength(50)
            .WithMessage("El número de colegiado no puede superar los 50 caracteres.")
            .When(x => x.CollegiateNumber is not null);

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty()
            .WithMessage("El identificador del usuario creador es obligatorio.");
    }
}
