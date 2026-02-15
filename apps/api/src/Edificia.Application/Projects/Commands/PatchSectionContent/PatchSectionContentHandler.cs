using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.PatchSectionContent;

/// <summary>
/// Handles PatchSectionContentCommand using EF Core (write-side).
/// Loads the Project aggregate, updates a specific section's content, and persists.
/// </summary>
public sealed class PatchSectionContentHandler : IRequestHandler<PatchSectionContentCommand, Result>
{
    private readonly IProjectRepository _repository;

    public PatchSectionContentHandler(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        PatchSectionContentCommand request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure(
                Error.NotFound("Project", $"No se encontró el proyecto con ID '{request.ProjectId}'."));
        }

        if (project.ContentTreeJson is null)
        {
            return Result.Failure(
                Error.NotFound("ContentTree",
                    $"El proyecto '{request.ProjectId}' no tiene un árbol de contenido inicializado."));
        }

        var updated = project.UpdateSectionContent(request.SectionId, request.Content);

        if (!updated)
        {
            return Result.Failure(
                Error.NotFound("Section",
                    $"No se encontró la sección '{request.SectionId}' en el árbol de contenido."));
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
