using Edificia.Domain.Constants;
using FluentValidation;

namespace Edificia.Application.Users.Commands.UpdateUser;

/// <summary>
/// FluentValidation validator for UpdateUserCommand.
/// </summary>
public sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    private static readonly string[] AllowedRoles =
        [AppRoles.Admin, AppRoles.Architect, AppRoles.Collaborator];

    public UpdateUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El identificador del usuario es obligatorio.");

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

        RuleFor(x => x.UpdatedByUserId)
            .NotEmpty()
            .WithMessage("El identificador del usuario que actualiza es obligatorio.");
    }
}
