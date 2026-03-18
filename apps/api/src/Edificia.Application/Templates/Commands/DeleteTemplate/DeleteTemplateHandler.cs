using Edificia.Application.Interfaces;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Templates.Commands.DeleteTemplate;

public sealed class DeleteTemplateHandler : IRequestHandler<DeleteTemplateCommand, Result>
{
    private readonly ITemplateRepository _templateRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteTemplateHandler> _logger;

    public DeleteTemplateHandler(
        ITemplateRepository templateRepository,
        IFileStorageService fileStorageService,
        ILogger<DeleteTemplateHandler> logger)
    {
        _templateRepository = templateRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
            if (template is null)
            {
                return Result.Failure(TemplateErrors.TemplateNotFound);
            }

            if (template.IsDefault)
            {
                return Result.Failure(TemplateErrors.CannotDeleteDefaultTemplate);
            }

            var removed = await _fileStorageService.DeleteFileAsync(template.StoragePath, cancellationToken);
            if (!removed)
            {
                _logger.LogWarning(
                    "Template binary was not found in storage for template {TemplateId} at {StoragePath}. Deleting metadata anyway.",
                    template.Id,
                    template.StoragePath);
            }

            _templateRepository.Remove(template);
            await _templateRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting template {TemplateId}",
                request.TemplateId);
            return Result.Failure(TemplateErrors.DeleteFailed);
        }
    }
}