using FluentValidation;

namespace Edificia.Application.Templates.Commands.UpdateTemplateMetadata;

public sealed class UpdateTemplateMetadataValidator : AbstractValidator<UpdateTemplateMetadataCommand>
{
    public UpdateTemplateMetadataValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .WithMessage("El identificador de la plantilla es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre de la plantilla es obligatorio.")
            .MaximumLength(200)
            .WithMessage("El nombre no puede superar los 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La descripción no puede superar los 1000 caracteres.")
            .When(x => x.Description is not null);
    }
}