using Edificia.Application.TemplateParams.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.TemplateParams.Commands.SetTemplateParamActivation;

public sealed record SetTemplateParamActivationCommand(
    Guid TemplateParamId,
    bool IsActive) : IRequest<Result>
{
    public static SetTemplateParamActivationCommand Create(
        Guid templateParamId,
        SetTemplateParamActivationRequest request)
        => new(templateParamId, request.IsActive);
}
