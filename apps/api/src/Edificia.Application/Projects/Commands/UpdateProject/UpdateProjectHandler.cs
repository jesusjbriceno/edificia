using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.UpdateProject;

/// <summary>
/// Handler for UpdateProjectCommand.
/// Loads the project, applies settings changes, and persists.
/// </summary>
public sealed class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, Result>
{
    private readonly IProjectRepository _projectRepository;

    public UpdateProjectHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure(
                Error.NotFound("Project.NotFound", "El proyecto no existe."));
        }

        project.UpdateSettings(
            title: request.Title,
            interventionType: request.InterventionType,
            isLoeRequired: request.IsLoeRequired,
            description: request.Description,
            address: request.Address,
            cadastralReference: request.CadastralReference,
            localRegulations: request.LocalRegulations);

        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
