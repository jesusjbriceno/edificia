using FluentValidation;

namespace Edificia.Application.Projects.Queries.GetProjects;

/// <summary>
/// Validates pagination and filter parameters for GetProjectsQuery.
/// </summary>
public sealed class GetProjectsValidator : AbstractValidator<GetProjectsQuery>
{
    private static readonly string[] AllowedStatuses =
        ["Draft", "InProgress", "PendingReview", "Completed", "Archived"];

    public GetProjectsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La página debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("El tamaño de página debe estar entre 1 y 50.");

        RuleFor(x => x.Status)
            .Must(s => AllowedStatuses.Contains(s!))
            .WithMessage("El estado no es válido. Valores permitidos: Draft, InProgress, PendingReview, Completed, Archived.")
            .When(x => x.Status is not null);
    }
}
