using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.ApproveProject;

/// <summary>
/// Handles ApproveProject: transitions project from PendingReview → Completed
/// and notifies the project creator.
/// </summary>
public sealed class ApproveProjectHandler : IRequestHandler<ApproveProjectCommand, Result>
{
    private readonly IProjectRepository _projectRepository;

    public ApproveProjectHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result> Handle(ApproveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
            return Result.Failure(
                Error.NotFound("Project.NotFound",
                    $"No se encontró el proyecto con ID '{request.ProjectId}'."));

        // Domain method enforces valid transitions; throws BusinessRuleException if invalid.
        project.Approve();

        // TODO: Notify project creator when CreatedByUserId is available
        // (pending merge of feature/project-ownership)

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
