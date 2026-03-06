using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Edificia.Application.Export.Services;

/// <summary>
/// Coordinates retrieval and resolution of active template placeholders.
/// </summary>
public sealed class TemplatePlaceholderService : ITemplatePlaceholderService
{
    private readonly ITemplateParamRepository _templateParamRepository;
    private readonly ITemplateParameterResolver _templateParameterResolver;
    private readonly ILogger<TemplatePlaceholderService> _logger;

    public TemplatePlaceholderService(
        ITemplateParamRepository templateParamRepository,
        ITemplateParameterResolver templateParameterResolver,
        ILogger<TemplatePlaceholderService> logger)
    {
        _templateParamRepository = templateParamRepository;
        _templateParameterResolver = templateParameterResolver;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<string, string>> ResolveAsync(
        Project project,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var activeParameters = await _templateParamRepository.GetActiveAsync(cancellationToken);
            if (activeParameters.Count == 0)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            return _templateParameterResolver.Resolve(project, activeParameters);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to resolve template placeholder parameters for project {ProjectId}. Continuing without parameter replacements.",
                project.Id);

            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
