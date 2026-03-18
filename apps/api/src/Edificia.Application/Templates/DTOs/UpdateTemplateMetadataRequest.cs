namespace Edificia.Application.Templates.DTOs;

public sealed record UpdateTemplateMetadataRequest(
    string Name,
    string? Description);