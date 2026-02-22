using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.ApproveProject;

/// <summary>
/// Approves a project pending review, transitioning it to Completed.
/// Only Admin/Root users can approve.
/// </summary>
public sealed record ApproveProjectCommand(Guid ProjectId) : IRequest<Result>;
