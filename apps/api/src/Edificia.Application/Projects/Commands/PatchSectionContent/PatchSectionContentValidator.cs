using FluentValidation;

namespace Edificia.Application.Projects.Commands.PatchSectionContent;

/// <summary>
/// Validates the PatchSectionContentCommand.
/// Ensures required fields are provided and content is within size limits.
/// </summary>
public sealed class PatchSectionContentValidator : AbstractValidator<PatchSectionContentCommand>
{
    /// <summary>
    /// Maximum allowed size for a single section content (512 KB).
    /// </summary>
    private const int MaxContentLength = 524_288;

    /// <summary>
    /// Maximum allowed length for a section identifier.
    /// </summary>
    private const int MaxSectionIdLength = 200;

    public PatchSectionContentValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es obligatorio.");

        RuleFor(x => x.SectionId)
            .NotEmpty()
            .WithMessage("El ID de la sección es obligatorio.")
            .MaximumLength(MaxSectionIdLength)
            .WithMessage($"El ID de la sección no puede superar {MaxSectionIdLength} caracteres.");

        RuleFor(x => x.Content)
            .NotNull()
            .WithMessage("El contenido de la sección es obligatorio.")
            .MaximumLength(MaxContentLength)
            .WithMessage($"El contenido de la sección no puede superar {MaxContentLength / 1024} KB.");
    }
}
