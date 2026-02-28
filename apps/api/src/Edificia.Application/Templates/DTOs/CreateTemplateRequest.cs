namespace Edificia.Application.Templates.DTOs;

public sealed record CreateTemplateRequest(
    string Name,
    string TemplateType,
    string? Description = null);
