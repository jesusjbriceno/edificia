using FluentValidation;

namespace Edificia.Application.TemplateParams.Commands.SetTemplateParamActivation;

public sealed class SetTemplateParamActivationValidator : AbstractValidator<SetTemplateParamActivationCommand>
{
    public SetTemplateParamActivationValidator()
    {
        RuleFor(x => x.TemplateParamId)
            .NotEmpty()
            .WithMessage("El identificador del parámetro es obligatorio.");
    }
}
