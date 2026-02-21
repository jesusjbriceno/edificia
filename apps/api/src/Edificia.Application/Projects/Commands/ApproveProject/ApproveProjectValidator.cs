using FluentValidation;

namespace Edificia.Application.Projects.Commands.ApproveProject;

public sealed class ApproveProjectValidator : AbstractValidator<ApproveProjectCommand>
{
    public ApproveProjectValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("El ID del proyecto es obligatorio.");
    }
}
