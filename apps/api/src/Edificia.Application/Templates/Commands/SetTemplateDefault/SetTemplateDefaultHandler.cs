using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Templates.Commands.SetTemplateDefault;

public sealed class SetTemplateDefaultHandler : IRequestHandler<SetTemplateDefaultCommand, Result>
{
    private readonly ITemplateRepository _templateRepository;
    private readonly ILogger<SetTemplateDefaultHandler> _logger;

    public SetTemplateDefaultHandler(
        ITemplateRepository templateRepository,
        ILogger<SetTemplateDefaultHandler> logger)
    {
        _templateRepository = templateRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(SetTemplateDefaultCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
            if (template is null)
            {
                return Result.Failure(TemplateErrors.TemplateNotFound);
            }

            if (!request.IsDefault)
            {
                template.ClearDefault();
                _templateRepository.Update(template);
                await _templateRepository.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }

            var currentDefault = await _templateRepository.GetDefaultByTypeAsync(template.TemplateType, cancellationToken);
            if (currentDefault is not null && currentDefault.Id != template.Id)
            {
                currentDefault.ClearDefault();
                _templateRepository.Update(currentDefault);
            }

            if (!template.IsAvailable)
            {
                template.SetAvailable(true);
            }

            template.MarkAsDefault();

            _templateRepository.Update(template);
            await _templateRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating default state for template {TemplateId}",
                request.TemplateId);
            return Result.Failure(TemplateErrors.DefaultStateFailed);
        }
    }
}