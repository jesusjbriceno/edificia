using Edificia.Domain.Enums;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.CreateProject;

/// <summary>
/// Command to create a new construction project.
/// Returns the new project's GUID on success.
/// CreatedByUserId is injected by the controller from JWT claims (not from request body).
/// </summary>
public sealed record CreateProjectCommand(
    string Title,
    InterventionType InterventionType,
    bool IsLoeRequired,
    string? Description = null,
    string? Address = null,
    string? CadastralReference = null,
    string? LocalRegulations = null) : IRequest<Result<Guid>>
{
    /// <summary>ID del usuario autenticado que crea el proyecto (inyectado desde JWT).</summary>
    public Guid CreatedByUserId { get; init; }

    public static explicit operator CreateProjectCommand(CreateProjectRequest r)
        => new(r.Title, r.InterventionType, r.IsLoeRequired,
               r.Description, r.Address, r.CadastralReference, r.LocalRegulations);
}
