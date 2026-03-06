using Edificia.Application.TemplateParams.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.TemplateParams.Queries.GetTemplateParams;

public sealed record GetTemplateParamsQuery(bool? IsActive = null)
    : IRequest<Result<IReadOnlyList<TemplateParamResponse>>>;
