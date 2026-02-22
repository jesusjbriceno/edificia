using Edificia.Domain.Enums;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.UpdateProject;

/// <summary>
/// Command to update a project's settings (title, intervention type, LOE, etc.).
/// </summary>
public sealed record UpdateProjectCommand(
    Guid ProjectId,
    string Title,
    InterventionType InterventionType,
    bool IsLoeRequired,
    string? Description = null,
    string? Address = null,
    string? CadastralReference = null,
    string? LocalRegulations = null) : IRequest<Result>
{
    public static UpdateProjectCommand Create(Guid projectId, UpdateProjectRequest r)
        => new(projectId, r.Title, r.InterventionType, r.IsLoeRequired,
               r.Description, r.Address, r.CadastralReference, r.LocalRegulations);
}
