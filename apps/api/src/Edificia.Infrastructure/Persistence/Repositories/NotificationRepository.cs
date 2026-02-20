using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;
using Edificia.Infrastructure.Persistence;

namespace Edificia.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of INotificationRepository.
/// </summary>
public sealed class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(EdificiaDbContext context) : base(context)
    {
    }
}
