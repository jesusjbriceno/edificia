using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Templates.Commands.ToggleTemplateStatus;

public sealed class ToggleTemplateStatusHandler : IRequestHandler<ToggleTemplateStatusCommand, Result>
{
    private readonly ITemplateRepository _templateRepository;
    private readonly ILogger<ToggleTemplateStatusHandler> _logger;

    public ToggleTemplateStatusHandler(
        ITemplateRepository templateRepository,
        ILogger<ToggleTemplateStatusHandler> logger)
    {
        _templateRepository = templateRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(ToggleTemplateStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
            if (template is null)
            {
                return Result.Failure(TemplateErrors.TemplateNotFound);
            }

            template.SetAvailable(request.IsActive);

            _templateRepository.Update(template);
            await _templateRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling template status for template {TemplateId}", request.TemplateId);
            return Result.Failure(TemplateErrors.ActivationFailed);
        }
    }
}
