using Edificia.Application.Templates.DTOs;
using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Templates.Queries.GetTemplates;

public sealed record GetTemplatesQuery(
    string? TemplateType = null,
    bool? IsActive = null) : IRequest<Result<IReadOnlyList<TemplateResponse>>>;
