using Edificia.Application.Interfaces;
using Edificia.Domain.Exceptions;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Templates.Commands.UpdateTemplateMetadata;

public sealed class UpdateTemplateMetadataHandler : IRequestHandler<UpdateTemplateMetadataCommand, Result>
{
    private readonly ITemplateRepository _templateRepository;
    private readonly ILogger<UpdateTemplateMetadataHandler> _logger;

    public UpdateTemplateMetadataHandler(
        ITemplateRepository templateRepository,
        ILogger<UpdateTemplateMetadataHandler> logger)
    {
        _templateRepository = templateRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTemplateMetadataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
            if (template is null)
            {
                return Result.Failure(TemplateErrors.TemplateNotFound);
            }

            template.Rename(request.Name, request.Description);

            _templateRepository.Update(template);
            await _templateRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex,
                "Business rule failed while updating template metadata for template {TemplateId}",
                request.TemplateId);
            return Result.Failure(TemplateErrors.InvalidMetadata(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating template metadata for template {TemplateId}",
                request.TemplateId);
            return Result.Failure(TemplateErrors.UpdateFailed);
        }
    }
}