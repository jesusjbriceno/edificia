using Edificia.Shared.Result;

namespace Edificia.Application.TemplateParams;

public static class TemplateParamErrors
{
    public static readonly Error NotFound =
        Error.NotFound("TemplateParam.NotFound", "El parámetro de plantilla no existe.");

    public static readonly Error ActivationFailed =
        Error.Failure("TemplateParam.ActivationFailed", "No se pudo actualizar el estado del parámetro de plantilla.");
}
