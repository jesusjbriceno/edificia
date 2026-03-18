using Edificia.Shared.Result;

namespace Edificia.Application.Templates;

public static class TemplateErrors
{
    public static readonly Error TemplateNotFound =
        Error.NotFound("Template.NotFound", "La plantilla no existe.");

    public static readonly Error StorageFailed =
        Error.Failure("Template.StorageFailed", "No se pudo guardar el archivo de la plantilla.");

    public static readonly Error ActivationFailed =
        Error.Failure("Template.ActivationFailed", "No se pudo actualizar el estado de la plantilla.");

    public static readonly Error AvailabilityFailed =
        Error.Failure("Template.AvailabilityFailed", "No se pudo actualizar la disponibilidad de la plantilla.");

    public static readonly Error DefaultStateFailed =
        Error.Failure("Template.DefaultStateFailed", "No se pudo actualizar la plantilla predeterminada.");

    public static readonly Error UpdateFailed =
        Error.Failure("Template.UpdateFailed", "No se pudo actualizar la plantilla.");

    public static readonly Error DeleteFailed =
        Error.Failure("Template.DeleteFailed", "No se pudo eliminar la plantilla.");

    public static readonly Error CannotDeleteDefaultTemplate =
        Error.Conflict("Template.CannotDeleteDefault", "No se puede eliminar una plantilla predeterminada.");

    public static Error InvalidMetadata(string details) =>
        Error.Validation("Template.InvalidMetadata", $"Metadatos de plantilla inválidos: {details}");

    public static Error InvalidFormat(string details) =>
        Error.Validation("Template.InvalidFormat", $"Formato de plantilla inválido: {details}");
}
