using Edificia.Domain.Enums;

namespace Edificia.Application.Projects.Commands.UpdateProject;

/// <summary>
/// API request DTO for updating a project's settings.
/// </summary>
public sealed record UpdateProjectRequest(
    string Title,
    InterventionType InterventionType,
    bool IsLoeRequired,
    string? Description = null,
    string? Address = null,
    string? CadastralReference = null,
    string? LocalRegulations = null);
