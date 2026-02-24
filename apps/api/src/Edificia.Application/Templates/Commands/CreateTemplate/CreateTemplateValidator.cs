using FluentValidation;

namespace Edificia.Application.Templates.Commands.CreateTemplate;

public sealed class CreateTemplateValidator : AbstractValidator<CreateTemplateCommand>
{
    private const int MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly string[] AllowedMimeTypes =
    [
        "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
        "application/octet-stream"
    ];

    public CreateTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre de la plantilla es obligatorio.")
            .MaximumLength(200)
            .WithMessage("El nombre no puede superar los 200 caracteres.");

        RuleFor(x => x.TemplateType)
            .NotEmpty()
            .WithMessage("El tipo de plantilla es obligatorio.")
            .MaximumLength(100)
            .WithMessage("El tipo de plantilla no puede superar los 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La descripción no puede superar los 1000 caracteres.")
            .When(x => x.Description is not null);

        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
            .WithMessage("El nombre del archivo es obligatorio.")
            .Must(fileName => fileName.EndsWith(".dotx", StringComparison.OrdinalIgnoreCase))
            .WithMessage("El archivo debe tener extensión .dotx.");

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .WithMessage("El tipo MIME es obligatorio.")
            .Must(mimeType => AllowedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase))
            .WithMessage("El tipo MIME del archivo no es válido para plantillas .dotx.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("El archivo debe tener contenido.")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("El tamaño del archivo no puede superar los 10MB.");

        RuleFor(x => x.FileContent)
            .NotNull()
            .WithMessage("El contenido del archivo es obligatorio.")
            .Must(content => content.Length > 0)
            .WithMessage("El contenido del archivo no puede estar vacío.")
            .Must((command, content) => content.LongLength == command.FileSizeBytes)
            .WithMessage("El tamaño del contenido no coincide con el tamaño del archivo.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty()
            .WithMessage("El usuario creador es obligatorio.");
    }
}
