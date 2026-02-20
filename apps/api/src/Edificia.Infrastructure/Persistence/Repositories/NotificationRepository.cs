using Edificia.Application.Interfaces;
using Edificia.Domain.Entities;

namespace Edificia.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of INotificationRepository (write-side).
/// Inherits standard CRUD from <see cref="BaseRepository{T}"/>.
/// </summary>
public sealed class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(EdificiaDbContext context) : base(context)
    {
    }
}
