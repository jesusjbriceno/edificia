using Edificia.Domain.Entities;

namespace Edificia.Application.Interfaces;

/// <summary>
/// Repository interface for Notification aggregate (write-side).
/// Extends <see cref="IBaseRepository{T}"/> with Notification-specific operations.
/// Defined in Application layer; implemented in Infrastructure.
/// </summary>
public interface INotificationRepository : IBaseRepository<Notification>
{
}
