using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.DeleteProject;

/// <summary>
/// Handler for DeleteProjectCommand.
/// Loads the project and removes it from the database.
/// </summary>
public sealed class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, Result>
{
    private readonly IProjectRepository _projectRepository;

    public DeleteProjectHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure(
                Error.NotFound("Project.NotFound", "El proyecto no existe."));
        }

        _projectRepository.Remove(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
