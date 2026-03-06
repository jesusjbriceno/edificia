namespace Edificia.Application.TemplateParams.DTOs;

public sealed record TemplateParamResponse(
    Guid Id,
    string Key,
    string DisplayName,
    string SourceCode,
    string? Formatter,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
