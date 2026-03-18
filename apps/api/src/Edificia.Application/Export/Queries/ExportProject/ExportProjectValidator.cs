using FluentValidation;

namespace Edificia.Application.Export.Queries.ExportProject;

public sealed class ExportProjectValidator : AbstractValidator<ExportProjectQuery>
{
    public ExportProjectValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("El identificador del proyecto es obligatorio.");

        RuleFor(x => x.TemplateId)
            .Must(templateId => !templateId.HasValue || templateId.Value != Guid.Empty)
            .WithMessage("El identificador de la plantilla no es válido.");

        RuleFor(x => x.OutputFileName)
            .MaximumLength(255)
            .WithMessage("El nombre del archivo no puede superar los 255 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.OutputFileName));
    }
}
