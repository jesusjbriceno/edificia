using Edificia.Domain.Entities;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Repository interface for Notification entity (write-side).
/// </summary>
public interface INotificationRepository : IBaseRepository<Notification>
{
    /// <summary>
    /// Gets all unread notifications for a specific user.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
