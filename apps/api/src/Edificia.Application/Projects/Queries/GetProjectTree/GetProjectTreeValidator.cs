using FluentValidation;

namespace Edificia.Application.Projects.Queries.GetProjectTree;

public sealed class GetProjectTreeValidator : AbstractValidator<GetProjectTreeQuery>
{
    public GetProjectTreeValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del proyecto es obligatorio.");
    }
}
