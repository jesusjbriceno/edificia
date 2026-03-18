using FluentValidation;

namespace Edificia.Application.TemplateParams.Queries.GetTemplateParams;

public sealed class GetTemplateParamsValidator : AbstractValidator<GetTemplateParamsQuery>
{
    public GetTemplateParamsValidator()
    {
    }
}
