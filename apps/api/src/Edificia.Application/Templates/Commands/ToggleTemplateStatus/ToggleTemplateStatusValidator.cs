using FluentValidation;

namespace Edificia.Application.Templates.Commands.ToggleTemplateStatus;

public sealed class ToggleTemplateStatusValidator : AbstractValidator<ToggleTemplateStatusCommand>
{
    public ToggleTemplateStatusValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage("El identificador de la plantilla es obligatorio.");
    }
}
