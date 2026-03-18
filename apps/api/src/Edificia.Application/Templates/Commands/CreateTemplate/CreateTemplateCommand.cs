using Edificia.Application.Templates.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Templates.Commands.CreateTemplate;

public sealed record CreateTemplateCommand(
    string Name,
    string TemplateType,
    string? Description,
    string OriginalFileName,
    string MimeType,
    long FileSizeBytes,
    byte[] FileContent,
    Guid CreatedByUserId) : IRequest<Result<Guid>>
{
    public static CreateTemplateCommand Create(
        Guid createdByUserId,
        CreateTemplateRequest request,
        string originalFileName,
        string mimeType,
        long fileSizeBytes,
        byte[] fileContent)
        => new(
            request.Name,
            request.TemplateType,
            request.Description,
            originalFileName,
            mimeType,
            fileSizeBytes,
            fileContent,
            createdByUserId);
}
