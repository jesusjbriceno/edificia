using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Edificia.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of INotificationRepository.
/// </summary>
public sealed class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(EdificiaDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);
    }
}
