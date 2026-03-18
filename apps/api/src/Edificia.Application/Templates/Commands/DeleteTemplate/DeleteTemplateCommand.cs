using Edificia.Shared.Result;
using MediatR;

namespace Edificia.Application.Templates.Commands.DeleteTemplate;

public sealed record DeleteTemplateCommand(Guid TemplateId) : IRequest<Result>;