using Edificia.Domain.Entities;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Repository interface for global template parameters.
/// </summary>
public interface ITemplateParamRepository : IBaseRepository<TemplateParam>
{
    Task<IReadOnlyList<TemplateParam>> GetActiveAsync(CancellationToken cancellationToken = default);
}
