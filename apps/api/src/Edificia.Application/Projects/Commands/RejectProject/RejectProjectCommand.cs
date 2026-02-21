using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.RejectProject;

/// <summary>
/// Rejects a project pending review, returning it to Draft.
/// Only Admin/Root users can reject. Requires a reason.
/// </summary>
public sealed record RejectProjectCommand(Guid ProjectId, string Reason) : IRequest<Result>;

/// <summary>
/// Request DTO for project rejection. Maps to RejectProjectCommand.
/// </summary>
public sealed record RejectProjectRequest(string Reason);
