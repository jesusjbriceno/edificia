namespace Edificia.Application.Interfaces;

/// <summary>
/// Service interface for querying users by role.
/// Implemented in Infrastructure using ASP.NET Identity.
/// </summary>
public interface IUserQueryService
{
    /// <summary>
    /// Gets the IDs of all active users that have the specified role.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetActiveUserIdsByRoleAsync(string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the IDs of all active users that have any of the specified roles.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetActiveUserIdsByRolesAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default);
}
