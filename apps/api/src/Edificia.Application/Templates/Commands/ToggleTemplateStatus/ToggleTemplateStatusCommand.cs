using Edificia.Application.Templates.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Templates.Commands.ToggleTemplateStatus;

public sealed record ToggleTemplateStatusCommand(
    Guid TemplateId,
    bool IsActive) : IRequest<Result>
{
    public static ToggleTemplateStatusCommand Create(Guid templateId, ToggleTemplateStatusRequest request)
        => new(templateId, request.IsActive);
}
