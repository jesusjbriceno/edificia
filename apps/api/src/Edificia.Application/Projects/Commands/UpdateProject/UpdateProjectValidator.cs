using FluentValidation;

namespace Edificia.Application.Projects.Commands.UpdateProject;

/// <summary>
/// FluentValidation validator for UpdateProjectCommand.
/// Runs automatically via the ValidationBehavior pipeline.
/// </summary>
public sealed class UpdateProjectValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("El identificador del proyecto es obligatorio.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("El título del proyecto es obligatorio.")
            .MaximumLength(300)
            .WithMessage("El título no puede superar los 300 caracteres.");

        RuleFor(x => x.InterventionType)
            .IsInEnum()
            .WithMessage("El tipo de intervención no es válido.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("La descripción no puede superar los 2000 caracteres.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("La dirección no puede superar los 500 caracteres.")
            .When(x => x.Address is not null);

        RuleFor(x => x.CadastralReference)
            .MaximumLength(100)
            .WithMessage("La referencia catastral no puede superar los 100 caracteres.")
            .When(x => x.CadastralReference is not null);

        RuleFor(x => x.LocalRegulations)
            .MaximumLength(5000)
            .WithMessage("La normativa local no puede superar los 5000 caracteres.")
            .When(x => x.LocalRegulations is not null);
    }
}
