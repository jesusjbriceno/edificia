using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.PatchSectionContent;

/// <summary>
/// Command to update the content of a specific section within a project's content tree.
/// Uses EF Core (write-side) to persist the partial JSONB update.
/// </summary>
public sealed record PatchSectionContentCommand(
    Guid ProjectId,
    string SectionId,
    string Content) : IRequest<Result>
{
    public static PatchSectionContentCommand Create(
        Guid projectId, string sectionId, UpdateSectionRequest r)
        => new(projectId, sectionId, r.Content);
}
