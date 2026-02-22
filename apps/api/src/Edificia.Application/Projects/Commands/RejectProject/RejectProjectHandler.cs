using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.RejectProject;

/// <summary>
/// Handles RejectProject: transitions project from PendingReview → Draft
/// and notifies the project creator with the rejection reason.
/// </summary>
public sealed class RejectProjectHandler : IRequestHandler<RejectProjectCommand, Result>
{
    private readonly IProjectRepository _projectRepository;

    public RejectProjectHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result> Handle(RejectProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
            return Result.Failure(
                Error.NotFound("Project.NotFound",
                    $"No se encontró el proyecto con ID '{request.ProjectId}'."));

        // Domain method enforces valid transitions; throws BusinessRuleException if invalid.
        project.Reject();

        // TODO: Notify project creator when CreatedByUserId is available
        // (pending merge of feature/project-ownership)

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
