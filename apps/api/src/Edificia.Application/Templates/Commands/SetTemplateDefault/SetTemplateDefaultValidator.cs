using FluentValidation;

namespace Edificia.Application.Templates.Commands.SetTemplateDefault;

public sealed class SetTemplateDefaultValidator : AbstractValidator<SetTemplateDefaultCommand>
{
    public SetTemplateDefaultValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage("El identificador de la plantilla es obligatorio.");
    }
}