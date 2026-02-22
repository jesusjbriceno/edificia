using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.DeleteProject;

/// <summary>
/// Command to delete a project from the system.
/// </summary>
public sealed record DeleteProjectCommand(Guid ProjectId) : IRequest<Result>;
