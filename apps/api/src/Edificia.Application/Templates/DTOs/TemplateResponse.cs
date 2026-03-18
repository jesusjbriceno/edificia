namespace Edificia.Application.Templates.DTOs;

public sealed record TemplateResponse(
    Guid Id,
    string Name,
    string? Description,
    string TemplateType,
    int Version,
    bool IsActive,
    string OriginalFileName,
    string MimeType,
    long FileSizeBytes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
