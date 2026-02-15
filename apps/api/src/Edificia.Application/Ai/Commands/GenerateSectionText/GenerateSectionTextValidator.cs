using FluentValidation;

namespace Edificia.Application.Ai.Commands.GenerateSectionText;

/// <summary>
/// Validates the GenerateSectionTextCommand.
/// Ensures required fields are provided and within size limits.
/// </summary>
public sealed class GenerateSectionTextValidator : AbstractValidator<GenerateSectionTextCommand>
{
    private const int MaxPromptLength = 10_000;
    private const int MaxContextLength = 50_000;
    private const int MaxSectionIdLength = 200;

    public GenerateSectionTextValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es obligatorio.");

        RuleFor(x => x.SectionId)
            .NotEmpty()
            .WithMessage("El ID de la sección es obligatorio.")
            .MaximumLength(MaxSectionIdLength)
            .WithMessage($"El ID de la sección no puede superar {MaxSectionIdLength} caracteres.");

        RuleFor(x => x.Prompt)
            .NotEmpty()
            .WithMessage("El prompt es obligatorio.")
            .MaximumLength(MaxPromptLength)
            .WithMessage($"El prompt no puede superar {MaxPromptLength} caracteres.");

        RuleFor(x => x.Context)
            .MaximumLength(MaxContextLength)
            .WithMessage($"El contexto no puede superar {MaxContextLength} caracteres.")
            .When(x => x.Context is not null);
    }
}
