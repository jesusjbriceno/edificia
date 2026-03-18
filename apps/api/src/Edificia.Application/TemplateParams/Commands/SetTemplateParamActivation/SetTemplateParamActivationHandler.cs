using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.TemplateParams.Commands.SetTemplateParamActivation;

public sealed class SetTemplateParamActivationHandler
    : IRequestHandler<SetTemplateParamActivationCommand, Result>
{
    private readonly ITemplateParamRepository _templateParamRepository;
    private readonly ILogger<SetTemplateParamActivationHandler> _logger;

    public SetTemplateParamActivationHandler(
        ITemplateParamRepository templateParamRepository,
        ILogger<SetTemplateParamActivationHandler> logger)
    {
        _templateParamRepository = templateParamRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(SetTemplateParamActivationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var parameter = await _templateParamRepository.GetByIdAsync(request.TemplateParamId, cancellationToken);
            if (parameter is null)
            {
                return Result.Failure(TemplateParamErrors.NotFound);
            }

            parameter.SetActive(request.IsActive);

            _templateParamRepository.Update(parameter);
            await _templateParamRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating activation for template parameter {TemplateParamId}",
                request.TemplateParamId);

            return Result.Failure(TemplateParamErrors.ActivationFailed);
        }
    }
}
