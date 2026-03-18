using FluentValidation;

namespace Edificia.Application.Templates.Queries.GetTemplates;

public sealed class GetTemplatesValidator : AbstractValidator<GetTemplatesQuery>
{
    public GetTemplatesValidator()
    {
        RuleFor(x => x.TemplateType)
            .MaximumLength(100)
            .WithMessage("El tipo de plantilla no puede superar los 100 caracteres.")
            .When(x => x.TemplateType is not null);
    }
}
