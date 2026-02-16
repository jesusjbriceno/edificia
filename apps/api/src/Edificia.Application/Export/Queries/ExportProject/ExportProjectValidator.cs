using FluentValidation;

namespace Edificia.Application.Export.Queries.ExportProject;

public sealed class ExportProjectValidator : AbstractValidator<ExportProjectQuery>
{
    public ExportProjectValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("El identificador del proyecto es obligatorio.");
    }
}
