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
}
