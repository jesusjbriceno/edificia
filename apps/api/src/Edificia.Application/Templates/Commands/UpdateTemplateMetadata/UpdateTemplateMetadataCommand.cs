using Edificia.Application.Templates.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Templates.Commands.UpdateTemplateMetadata;

public sealed record UpdateTemplateMetadataCommand(
    Guid TemplateId,
    string Name,
    string? Description) : IRequest<Result>
{
    public static UpdateTemplateMetadataCommand Create(Guid templateId, UpdateTemplateMetadataRequest request)
        => new(templateId, request.Name, request.Description);
}