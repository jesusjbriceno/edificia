using Edificia.Domain.Enums;

namespace Edificia.Application.Projects.Commands.CreateProject;

/// <summary>
/// API request DTO for creating a new project.
/// </summary>
public sealed record CreateProjectRequest(
    string Title,
    InterventionType InterventionType,
    bool IsLoeRequired,
    string? Description = null,
    string? Address = null,
    string? CadastralReference = null,
    string? LocalRegulations = null);
