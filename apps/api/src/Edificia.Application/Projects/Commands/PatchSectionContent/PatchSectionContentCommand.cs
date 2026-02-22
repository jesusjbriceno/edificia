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
    /// <summary>
    /// Maps the DTO fields. <see cref="ProjectId"/> and <see cref="SectionId"/> must be
    /// enriched from the route params in the controller using a <c>with</c> expression.
    /// </summary>
    public static explicit operator PatchSectionContentCommand(UpdateSectionRequest r)
        => new(Guid.Empty, string.Empty, r.Content);
}
