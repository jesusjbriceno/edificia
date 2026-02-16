using FluentValidation;

namespace Edificia.Application.Projects.Commands.UpdateProjectTree;

/// <summary>
/// Validates the UpdateProjectTreeCommand.
/// Ensures the JSON content is provided and not excessively large.
/// </summary>
public sealed class UpdateProjectTreeValidator : AbstractValidator<UpdateProjectTreeCommand>
{
    /// <summary>
    /// Maximum allowed size for the content tree JSON (1 MB).
    /// </summary>
    private const int MaxContentTreeLength = 1_048_576;

    public UpdateProjectTreeValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es obligatorio.");

        RuleFor(x => x.ContentTreeJson)
            .NotEmpty()
            .WithMessage("El contenido del árbol normativo es obligatorio.")
            .MaximumLength(MaxContentTreeLength)
            .WithMessage($"El contenido del árbol normativo no puede superar {MaxContentTreeLength / 1024} KB.");
    }
}
