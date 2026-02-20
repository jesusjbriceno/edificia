using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.UpdateProjectTree;

/// <summary>
/// Command to update the content tree JSON of a project.
/// Uses EF Core (write-side) to persist the JSONB content.
/// </summary>
public sealed record UpdateProjectTreeCommand(
    Guid ProjectId,
    string ContentTreeJson) : IRequest<Result>
{
    /// <summary>
    /// Maps the DTO fields. <see cref="ProjectId"/> must be enriched
    /// from the route param in the controller using a <c>with</c> expression.
    /// </summary>
    public static explicit operator UpdateProjectTreeCommand(UpdateProjectTreeRequest r)
        => new(Guid.Empty, r.ContentTreeJson);
}
