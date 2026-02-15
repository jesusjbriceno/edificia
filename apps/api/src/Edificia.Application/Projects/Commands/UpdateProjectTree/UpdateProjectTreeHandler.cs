using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.UpdateProjectTree;

/// <summary>
/// Handles UpdateProjectTreeCommand using EF Core (write-side).
/// Loads the Project aggregate, updates the content tree, and persists.
/// </summary>
public sealed class UpdateProjectTreeHandler : IRequestHandler<UpdateProjectTreeCommand, Result>
{
    private readonly IProjectRepository _repository;

    public UpdateProjectTreeHandler(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        UpdateProjectTreeCommand request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure(
                Error.NotFound("Project", $"No se encontró el proyecto con ID '{request.ProjectId}'."));
        }

        project.UpdateContentTree(request.ContentTreeJson);

        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
