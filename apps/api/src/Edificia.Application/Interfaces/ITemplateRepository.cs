using Edificia.Domain.Entities;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Repository interface for AppTemplate aggregate (write-side).
/// </summary>
public interface ITemplateRepository : IBaseRepository<AppTemplate>
{
    Task<AppTemplate?> GetActiveByTypeAsync(
        string templateType,
        CancellationToken cancellationToken = default);

    Task<AppTemplate?> GetDefaultByTypeAsync(
        string templateType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppTemplate>> GetAvailableByTypeAsync(
        string templateType,
        CancellationToken cancellationToken = default);

    Task<int> CountByTypeAsync(
        string templateType,
        CancellationToken cancellationToken = default);
}
