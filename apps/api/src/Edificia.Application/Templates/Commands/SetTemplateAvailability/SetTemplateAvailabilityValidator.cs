using FluentValidation;

namespace Edificia.Application.Templates.Commands.SetTemplateAvailability;

public sealed class SetTemplateAvailabilityValidator : AbstractValidator<SetTemplateAvailabilityCommand>
{
    public SetTemplateAvailabilityValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage("El identificador de la plantilla es obligatorio.");
    }
}