using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Templates.Commands.SetTemplateAvailability;

public sealed class SetTemplateAvailabilityHandler : IRequestHandler<SetTemplateAvailabilityCommand, Result>
{
    private readonly ITemplateRepository _templateRepository;
    private readonly ILogger<SetTemplateAvailabilityHandler> _logger;

    public SetTemplateAvailabilityHandler(
        ITemplateRepository templateRepository,
        ILogger<SetTemplateAvailabilityHandler> logger)
    {
        _templateRepository = templateRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(SetTemplateAvailabilityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
            if (template is null)
            {
                return Result.Failure(TemplateErrors.TemplateNotFound);
            }

            template.SetAvailable(request.IsAvailable);

            _templateRepository.Update(template);
            await _templateRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating availability for template {TemplateId}",
                request.TemplateId);
            return Result.Failure(TemplateErrors.AvailabilityFailed);
        }
    }
}