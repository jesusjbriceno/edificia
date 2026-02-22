using Edificia.Domain.Enums;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.CreateProject;

/// <summary>
/// Command to create a new construction project.
/// Returns the new project's GUID on success.
/// </summary>
public sealed record CreateProjectCommand(
    string Title,
    InterventionType InterventionType,
    bool IsLoeRequired,
    Guid CreatedByUserId,
    string? Description = null,
    string? Address = null,
    string? CadastralReference = null,
    string? LocalRegulations = null) : IRequest<Result<Guid>>
{
    public static CreateProjectCommand Create(Guid createdByUserId, CreateProjectRequest r)
        => new(r.Title, r.InterventionType, r.IsLoeRequired, createdByUserId,
               r.Description, r.Address, r.CadastralReference, r.LocalRegulations);
}
