using Edificia.Application.Interfaces;
using Edificia.Domain.Constants;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Projects.Commands.SubmitForReview;

/// <summary>
/// Handles SubmitForReview: transitions project to PendingReview
/// and creates notifications for all active Admin/Root users.
/// </summary>
public sealed class SubmitForReviewHandler : IRequestHandler<SubmitForReviewCommand, Result>
{
    private readonly IProjectRepository _projectRepository;
    private readonly INotificationService _notificationService;
    private readonly IUserQueryService _userQueryService;

    public SubmitForReviewHandler(
        IProjectRepository projectRepository,
        INotificationService notificationService,
        IUserQueryService userQueryService)
    {
        _projectRepository = projectRepository;
        _notificationService = notificationService;
        _userQueryService = userQueryService;
    }

    public async Task<Result> Handle(SubmitForReviewCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project is null)
            return Result.Failure(
                Error.NotFound("Project.NotFound",
                    $"No se encontró el proyecto con ID '{request.ProjectId}'."));

        // Domain method enforces valid transitions; throws BusinessRuleException if invalid.
        project.SubmitForReview();

        // Notify all active Admin and Root users
        var reviewerIds = await _userQueryService.GetActiveUserIdsByRolesAsync(
            [AppRoles.Admin, AppRoles.Root], cancellationToken);

        await _notificationService.CreateForManyAsync(
            reviewerIds,
            "Proyecto enviado a revisión",
            $"El proyecto '{project.Title}' ha sido enviado a revisión y requiere su aprobación.",
            cancellationToken);

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
