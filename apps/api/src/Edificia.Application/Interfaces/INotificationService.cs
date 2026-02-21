namespace Edificia.Application.Interfaces;

/// <summary>
/// Service interface for creating domain notifications.
/// Implemented in Infrastructure to avoid Application→Infrastructure dependency.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates a notification for a specific user.
    /// The notification is persisted when SaveChanges is called on the unit of work.
    /// </summary>
    Task CreateAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates notifications for multiple users with the same message.
    /// </summary>
    Task CreateForManyAsync(IEnumerable<Guid> userIds, string title, string message, CancellationToken cancellationToken = default);
}
