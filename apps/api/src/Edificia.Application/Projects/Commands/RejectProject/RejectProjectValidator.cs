using FluentValidation;

namespace Edificia.Application.Projects.Commands.RejectProject;

public sealed class RejectProjectValidator : AbstractValidator<RejectProjectCommand>
{
    public RejectProjectValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("El ID del proyecto es obligatorio.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo del rechazo es obligatorio.")
            .MaximumLength(2000).WithMessage("El motivo no puede superar los 2000 caracteres.");
    }
}
