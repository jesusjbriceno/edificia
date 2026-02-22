using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Infrastructure.Persistence;

namespace Edificia.Infrastructure.Identity;

/// <summary>
/// Infrastructure implementation of INotificationService using EF Core.
/// Notifications are tracked in the same DbContext and persisted when SaveChanges is called.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly EdificiaDbContext _dbContext;

    public NotificationService(EdificiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default)
    {
        var notification = Notification.Create(userId, title, message);
        await _dbContext.Set<Notification>().AddAsync(notification, cancellationToken);
    }

    public async Task CreateForManyAsync(IEnumerable<Guid> userIds, string title, string message, CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds)
        {
            var notification = Notification.Create(userId, title, message);
            await _dbContext.Set<Notification>().AddAsync(notification, cancellationToken);
        }
    }
}
