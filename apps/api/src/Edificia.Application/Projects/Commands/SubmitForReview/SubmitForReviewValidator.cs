using FluentValidation;

namespace Edificia.Application.Projects.Commands.SubmitForReview;

public sealed class SubmitForReviewValidator : AbstractValidator<SubmitForReviewCommand>
{
    public SubmitForReviewValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("El ID del proyecto es obligatorio.");
    }
}
