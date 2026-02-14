using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.CreateProject;

/// <summary>
/// Handler for CreateProjectCommand.
/// Creates a new Project entity and persists it via the repository.
/// </summary>
public sealed class CreateProjectHandler : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    private readonly IProjectRepository _projectRepository;

    public CreateProjectHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = Project.Create(
            title: request.Title,
            interventionType: request.InterventionType,
            isLoeRequired: request.IsLoeRequired,
            description: request.Description,
            address: request.Address,
            cadastralReference: request.CadastralReference,
            localRegulations: request.LocalRegulations);

        await _projectRepository.AddAsync(project, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(project.Id);
    }
}
