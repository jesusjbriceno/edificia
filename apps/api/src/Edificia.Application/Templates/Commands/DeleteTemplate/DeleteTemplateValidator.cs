using FluentValidation;

namespace Edificia.Application.Templates.Commands.DeleteTemplate;

public sealed class DeleteTemplateValidator : AbstractValidator<DeleteTemplateCommand>
{
    public DeleteTemplateValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage("El identificador de la plantilla es obligatorio.");
    }
}