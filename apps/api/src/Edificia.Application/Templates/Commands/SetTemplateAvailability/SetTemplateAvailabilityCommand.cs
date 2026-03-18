using Edificia.Application.Templates.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Templates.Commands.SetTemplateAvailability;

public sealed record SetTemplateAvailabilityCommand(
    Guid TemplateId,
    bool IsAvailable) : IRequest<Result>
{
    public static SetTemplateAvailabilityCommand Create(Guid templateId, SetTemplateAvailabilityRequest request)
        => new(templateId, request.IsAvailable);
}