using Edificia.Application.Templates.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Templates.Commands.SetTemplateDefault;

public sealed record SetTemplateDefaultCommand(
    Guid TemplateId,
    bool IsDefault) : IRequest<Result>
{
    public static SetTemplateDefaultCommand Create(Guid templateId, SetTemplateDefaultRequest request)
        => new(templateId, request.IsDefault);
}