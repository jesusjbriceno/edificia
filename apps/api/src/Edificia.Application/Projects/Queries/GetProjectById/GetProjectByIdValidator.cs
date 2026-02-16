using FluentValidation;

namespace Edificia.Application.Projects.Queries.GetProjectById;

public sealed class GetProjectByIdValidator : AbstractValidator<GetProjectByIdQuery>
{
    public GetProjectByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del proyecto es obligatorio.");
    }
}
