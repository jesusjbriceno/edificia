using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Templates.Commands.CreateTemplate;

public sealed class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand, Result<Guid>>
{
    private readonly ITemplateRepository _templateRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<CreateTemplateHandler> _logger;

    public CreateTemplateHandler(
        ITemplateRepository templateRepository,
        IFileStorageService fileStorageService,
        ILogger<CreateTemplateHandler> logger)
    {
        _templateRepository = templateRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await using var fileStream = new MemoryStream(request.FileContent, writable: false);
            var storagePath = await _fileStorageService.SaveFileAsync(
                fileStream,
                request.OriginalFileName,
                request.TemplateType,
                cancellationToken);

            var template = AppTemplate.Create(
                name: request.Name,
                description: request.Description,
                templateType: request.TemplateType,
                storagePath: storagePath,
                originalFileName: request.OriginalFileName,
                mimeType: request.MimeType,
                fileSizeBytes: request.FileSizeBytes,
                createdByUserId: request.CreatedByUserId);

            var activeTemplate = await _templateRepository.GetActiveByTypeAsync(request.TemplateType, cancellationToken);
            var hasActiveTemplate = activeTemplate is not null;

            if (!hasActiveTemplate)
            {
                template.Activate();
            }

            var currentCount = await _templateRepository.CountByTypeAsync(request.TemplateType, cancellationToken);
            for (var i = 1; i <= currentCount; i++)
            {
                template.PublishNewVersion(
                    template.StoragePath,
                    template.OriginalFileName,
                    template.MimeType,
                    template.FileSizeBytes);
            }

            if (hasActiveTemplate)
            {
                template.Deactivate();
            }

            await _templateRepository.AddAsync(template, cancellationToken);
            await _templateRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(template.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating template for type {TemplateType} and name {Name}",
                request.TemplateType,
                request.Name);
            return Result.Failure<Guid>(TemplateErrors.StorageFailed);
        }
    }
}
